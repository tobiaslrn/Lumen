use embassy_time::Duration;
use pio::ArrayVec;

use super::{
    bytestreamreader::{ByteStreamReader, MessageDeserializer},
    message_id::MessageId,
    rgb8::Rgb8,
    DeserializationResult,
};

#[derive(Debug)]
pub enum MessageKind {
    Empty,
    KeepAlive {
        duration: Duration,
    },
    LedState {
        strip_id: u8,
        led_values: ArrayVec<Rgb8, 400>,
    },
}

impl MessageDeserializer for MessageKind {
    type Result = DeserializationResult<Self>;

    fn deserialize_from(reader: &mut ByteStreamReader) -> Self::Result {
        let kind = reader.u16();
        let Ok(msg_id) = MessageId::try_from(kind) else {
            return Err(());
        };

        let message = match msg_id {
            MessageId::Empty => MessageKind::Empty,
            MessageId::KeepAlive => {
                let keepalive_for = reader.u32();
                let duration = Duration::from_millis(keepalive_for as u64);
                MessageKind::KeepAlive { duration }
            }
            MessageId::LedState => {
                let id = 0;
                let led_values_cnt = reader.u16();
                let mut led_values = ArrayVec::new();

                for _ in 0..led_values_cnt {
                    let rgb = Rgb8::deserialize_from(reader)?;
                    led_values.push(rgb);
                }

                MessageKind::LedState {
                    strip_id: id,
                    led_values,
                }
            }
        };

        Ok(message)
    }
}
