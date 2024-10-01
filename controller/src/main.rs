#![no_std]
#![no_main]

pub mod atomic_channel;
pub mod message_controller;
pub mod messages;
pub mod ws2812;

use crate::messages::rgb8::Rgb8;
use arrayvec::ArrayVec;
use atomic_channel::AtomicChannel;
use cyw43::JoinOptions;
use cyw43_pio::PioSpi;
use defmt::info;
use defmt::*;
use embassy_executor::Executor;
use embassy_executor::Spawner;
use embassy_net::udp::PacketMetadata;
use embassy_net::udp::UdpSocket;
use embassy_net::Ipv4Address;
use embassy_net::Ipv4Cidr;
use embassy_net::Stack;
use embassy_net::StackResources;
use embassy_net::StaticConfigV4;
use embassy_rp::bind_interrupts;
use embassy_rp::clocks::RoscRng;
use embassy_rp::gpio::Level;
use embassy_rp::gpio::Output;
use embassy_rp::multicore::{self, spawn_core1};
use embassy_rp::peripherals::DMA_CH0;
use embassy_rp::peripherals::PIO0;
use embassy_rp::peripherals::PIO1;
use embassy_rp::pio::Pio;
use embassy_sync::blocking_mutex::raw::CriticalSectionRawMutex;
use embassy_time::Duration;
use embassy_time::Timer;
use heapless::Vec;
use message_controller::MessageController;
use messages::bytestreamreader::ByteStreamReader;
use messages::bytestreamreader::MessageDeserializer;
use messages::ControllerMessage;
use rand::RngCore;
use static_assertions::const_assert;
use static_cell::StaticCell;
use ws2812::Ws2812;
use {defmt_rtt as _, panic_probe as _};

bind_interrupts!(struct Irqs {
    PIO0_IRQ_0 => embassy_rp::pio::InterruptHandler<PIO0>;
    PIO1_IRQ_0 => embassy_rp::pio::InterruptHandler<PIO1>;
});

const WIFI_NETWORK: &str = env!("SSID");
const WIFI_PASSWORD: &str = env!("PASSWORD");
const RECV_PORT_STR: &str = env!("NET_RECV_PORT");
const NET_ADDRESS_STR: &str = env!("NET_ADDRESS");
const NET_GATEWAY_STR: &str = env!("NET_GATEWAY");
const RECV_PORT: u16 = parse_u16(RECV_PORT_STR);
const LED_MAX: usize = 400;

// env variables have to be set in .cargo/config.toml
const_assert!(!WIFI_NETWORK.is_empty());
const_assert!(!WIFI_PASSWORD.is_empty());
const_assert!(!RECV_PORT_STR.is_empty());
const_assert!(!NET_ADDRESS_STR.is_empty());
const_assert!(!NET_GATEWAY_STR.is_empty());

const NET_FW: &[u8] = include_bytes!("../cyw43-firmware/43439A0.bin");
const NET_CLM: &[u8] = include_bytes!("../cyw43-firmware/43439A0_clm.bin");

static mut CORE1_STACK: multicore::Stack<32000> = multicore::Stack::new();
static EXECUTOR1: StaticCell<Executor> = StaticCell::new();

static CYW43_STATE: StaticCell<cyw43::State> = StaticCell::new();
static NET_STACK_RESOURCES: StaticCell<StackResources<10>> = StaticCell::new();

pub type MUTEX = CriticalSectionRawMutex;

// Use static channels to communicate between tasks
static ATOM_LED_STATE: AtomicChannel<MUTEX, ArrayVec<Rgb8, LED_MAX>> = AtomicChannel::new();
static ATOM_KEEP_ALIVE: AtomicChannel<MUTEX, Duration> = AtomicChannel::new();

macro_rules! var_info {
    ($var:ident) => {
        (stringify!($var), $var)
    };
}

#[embassy_executor::main]
async fn main(spawner: Spawner) {
    info!(
        "Starting with env vars:\n\t- {}\n\t- {}\n\t- {}\n\t- {}\n\t- {}",
        var_info!(WIFI_NETWORK),
        var_info!(WIFI_PASSWORD),
        var_info!(RECV_PORT),
        var_info!(NET_ADDRESS_STR),
        var_info!(NET_GATEWAY_STR)
    );

    let net_address = parse_ip_v4(NET_ADDRESS_STR);
    let net_gateway = parse_ip_v4(NET_GATEWAY_STR);

    let mut rng = RoscRng;
    let p = embassy_rp::init(Default::default());

    let mut pio_leds = Pio::new(p.PIO1, Irqs);
    let ws2812 = Ws2812::new(&mut pio_leds.common, pio_leds.sm0, p.DMA_CH1, p.PIN_12);

    spawn_core1(
        p.CORE1,
        unsafe { &mut *core::ptr::addr_of_mut!(CORE1_STACK) },
        move || {
            let ex1 = EXECUTOR1.init(Executor::new());
            ex1.run(|spawner| {
                spawner.must_spawn(keep_alive_task());
                spawner.must_spawn(write_led_strip_task(ws2812));
                info!("Finished spawning tasks for core 1");
            });
        },
    );

    let pwr = Output::new(p.PIN_23, Level::Low);
    let cs = Output::new(p.PIN_25, Level::High);
    let mut pio = Pio::new(p.PIO0, Irqs);
    let spi = PioSpi::new(
        &mut pio.common,
        pio.sm0,
        pio.irq0,
        cs,
        p.PIN_24,
        p.PIN_29,
        p.DMA_CH0,
    );

    let cyw43_state = CYW43_STATE.init(cyw43::State::new());
    let (net_device, mut cyw43_control, cyw43_runner) =
        cyw43::new(cyw43_state, pwr, spi, NET_FW).await;

    // Start the CYW43 WIFI Chip task
    spawner.must_spawn(cyw43_task(cyw43_runner));

    cyw43_control.init(NET_CLM).await;
    cyw43_control
        .set_power_management(cyw43::PowerManagementMode::PowerSave)
        .await;

    let static_wifi_config = embassy_net::Config::ipv4_static(StaticConfigV4 {
        address: Ipv4Cidr::new(net_address, 24),
        dns_servers: Vec::new(),
        gateway: Some(net_gateway),
    });
    let (net_stack, net_runner) = embassy_net::new(
        net_device,
        static_wifi_config,
        NET_STACK_RESOURCES.init(StackResources::new()),
        rng.next_u64(),
    );

    // Start the network stack
    spawner.must_spawn(net_task(net_runner));

    // Start the Lumen UDP message handler
    spawner.must_spawn(handle_udp_messages_task(net_stack));

    info!("Finished spawning tasks for core 0");

    loop {
        // Main loop to handle wifi reconnections
        net_stack.wait_link_down().await;
        join_wifi(
            &mut cyw43_control,
            WIFI_NETWORK,
            WIFI_PASSWORD,
            Duration::from_millis(500),
        )
        .await;
        net_stack.wait_link_up().await;
    }
}

#[embassy_executor::task]
async fn handle_udp_messages_task(stack: Stack<'static>) -> ! {
    let mut rx_buffer = [0; 4096];
    let mut rx_meta = [PacketMetadata::EMPTY; 16];
    let mut tx_buffer = [0; 0];
    let mut tx_meta = [PacketMetadata::EMPTY; 0];

    let mut udp_socket = UdpSocket::new(
        stack,
        &mut rx_meta,
        &mut rx_buffer,
        &mut tx_meta,
        &mut tx_buffer,
    );

    udp_socket.bind(RECV_PORT).unwrap();

    let mut msg_controller = MessageController::new();
    let mut message_buffer = [0; 2048];
    loop {
        match udp_socket.recv_from(&mut message_buffer).await {
            Err(e) => {
                warn!("error receiving message {}", e);
            }
            Ok((n, _)) => {
                let read = &message_buffer[0..n];
                let mut reader = ByteStreamReader::new(read);
                let decoded = ControllerMessage::deserialize_from(&mut reader);
                if decoded.is_err() {
                    error!("Error deserializing message");
                    continue;
                }
                msg_controller.handle_msg_lumen(decoded.unwrap()).await;
            }
        }
    }
}

#[embassy_executor::task]
async fn write_led_strip_task(mut ws: Ws2812<'static, PIO1, 0, LED_MAX>) -> ! {
    loop {
        let buffer = ATOM_LED_STATE.recv_item().await;
        ws.write(&buffer).await;
    }
}

/// The controller expects a KEEP_ALIVE message in intervals to keep the strip on or else it will turn off the LED strip.
#[embassy_executor::task]
async fn keep_alive_task() -> ! {
    let mut blank_buffer: ArrayVec<Rgb8, LED_MAX> = ArrayVec::new();
    for _ in 0..blank_buffer.capacity() {
        blank_buffer.push(Rgb8 { r: 0, g: 0, b: 0 })
    }
    let wait_for = Duration::from_millis(800);
    loop {
        let keepalive = ATOM_KEEP_ALIVE.recv_with_timeout(wait_for).await;
        match keepalive {
            Some(alive_duration) => {
                Timer::after(alive_duration).await;
            }
            None => {
                ATOM_LED_STATE.send(blank_buffer.clone()).await;
            }
        }
    }
}

#[embassy_executor::task]
async fn cyw43_task(
    runner: cyw43::Runner<'static, Output<'static>, PioSpi<'static, PIO0, 0, DMA_CH0>>,
) -> ! {
    runner.run().await
}

#[embassy_executor::task]
async fn net_task(mut runner: embassy_net::Runner<'static, cyw43::NetDriver<'static>>) -> ! {
    runner.run().await
}

/// Join a wifi network with the given ssid and password. Retries on failure.
async fn join_wifi(
    net_control: &mut cyw43::Control<'static>,
    ssid: &str,
    password: &str,
    time_until_retry: Duration,
) {
    let join_options = JoinOptions::new(password.as_bytes());
    loop {
        match net_control.join(ssid, join_options.clone()).await {
            Ok(_) => {
                info!("Successfully joined wifi");
                return;
            }
            Err(err) => {
                warn!("Failed to join wifi: {:?}", err.status);
            }
        }

        info!("Retrying wifi join in 500ms...");
        Timer::after(time_until_retry).await;
    }
}

const fn parse_u16(s: &'static str) -> u16 {
    let mut bytes = s.as_bytes();
    let mut val = 0;
    while let [byte, rest @ ..] = bytes {
        core::assert!(b'0' <= *byte && *byte <= b'9', "invalid digit");
        val = val * 10 + (*byte - b'0') as u16;
        bytes = rest;
    }
    val
}

fn parse_ip_v4(s: &str) -> Ipv4Address {
    let mut bytes = s.split('.').map(|b| b.parse::<u8>().unwrap());
    Ipv4Address::new(
        bytes.next().unwrap(),
        bytes.next().unwrap(),
        bytes.next().unwrap(),
        bytes.next().unwrap(),
    )
}
