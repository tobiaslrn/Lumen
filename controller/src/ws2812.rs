use crate::messages::rgb8::Rgb8;
use embassy_rp::clocks::{self};
use embassy_rp::dma::{AnyChannel, Channel};
use embassy_rp::pio::{
    Common, Config, FifoJoin, Instance, PioPin, ShiftConfig, ShiftDirection, StateMachine,
};
use embassy_rp::{Peripheral, PeripheralRef};
use embassy_time::Timer;
use fixed::types::U24F8;
use fixed_macro::fixed;

use {defmt_rtt as _, panic_probe as _};

pub struct Ws2812<'d, P: Instance, const SM: usize, const LEDS: usize> {
    dma: PeripheralRef<'d, AnyChannel>,
    sm: StateMachine<'d, P, SM>,
    buffer: [u32; LEDS],
}

impl<'d, P: Instance, const SM: usize, const LEDS: usize> Ws2812<'d, P, SM, LEDS> {
    pub fn new(
        pio: &mut Common<'d, P>,
        mut sm: StateMachine<'d, P, SM>,
        dma: impl Peripheral<P = impl Channel> + 'd,
        data_pin: impl PioPin,
    ) -> Self {
        let side_set = pio::SideSet::new(false, 1, false);

        let mut a: pio::Assembler<32> = pio::Assembler::new_with_side_set(side_set);

        const T1: u8 = 2; // start bit
        const T2: u8 = 5; // data bit
        const T3: u8 = 3; // stop bit
        const CYCLES_PER_BIT: u32 = (T1 + T2 + T3) as u32;

        let mut wrap_target = a.label();
        a.set_with_side_set(pio::SetDestination::PINDIRS, 1, 0);
        a.bind(&mut wrap_target);
        // Do stop bit
        a.out_with_delay_and_side_set(pio::OutDestination::X, 1, T3 - 1, 0);

        let mut do_zero = a.label();

        // Do start bit
        a.jmp_with_delay_and_side_set(pio::JmpCondition::XIsZero, &mut do_zero, T1 - 1, 1);
        // Do data bit = 1
        a.jmp_with_delay_and_side_set(pio::JmpCondition::Always, &mut wrap_target, T2 - 1, 1);
        a.bind(&mut do_zero);

        let mut wrap_source = a.label();

        // Do data bit = 0
        a.nop_with_delay_and_side_set(T2 - 1, 0);
        a.bind(&mut wrap_source);

        let prg = a.assemble_with_wrap(wrap_source, wrap_target);
        let mut cfg = Config::default();

        // Pin config
        let out_pin = pio.make_pio_pin(data_pin);
        cfg.set_out_pins(&[&out_pin]);
        cfg.set_set_pins(&[&out_pin]);

        cfg.use_program(&pio.load_program(&prg), &[&out_pin]);

        let clock_freq = U24F8::from_num(clocks::clk_sys_freq() / 1000);
        let ws2812_freq = fixed!(800: U24F8);
        let bit_freq = ws2812_freq * CYCLES_PER_BIT;
        cfg.clock_divider = clock_freq / bit_freq;

        // FIFO config
        cfg.fifo_join = FifoJoin::TxOnly;
        cfg.shift_out = ShiftConfig {
            auto_fill: true,
            threshold: 24,
            direction: ShiftDirection::Left,
        };

        sm.set_config(&cfg);
        sm.set_enable(true);

        Self {
            sm,
            dma: dma.into_ref().map_into(),
            buffer: [0u32; LEDS],
        }
    }

    pub async fn write(&mut self, colors: &[Rgb8]) {
        for (w, &Rgb8 { r, g, b }) in self.buffer.iter_mut().zip(colors.iter()) {
            *w = (u32::from(g) << 24) | (u32::from(r) << 16) | (u32::from(b) << 8);
        }

        let max_elements = self.buffer.len().min(colors.len());
        self.sm
            .tx()
            .dma_push(self.dma.reborrow(), &self.buffer[..max_elements])
            .await;

        Timer::after_micros(300).await;
    }
}
