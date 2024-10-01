use crate::messages::bytestreamreader::ByteStreamReader;
use crate::messages::bytestreamreader::MessageDeserializer;
use crate::messages::message_id::MessageId;
use crate::messages::message_kind::MessageKind;
use crate::messages::ControllerMessage;
use crate::set_keep_alive_buffer;
use crate::set_led_state_buffer;
use defmt::error;
use heapless::FnvIndexMap;

#[derive(Clone, Default)]
pub struct MessageController {
    timestamp_map: FnvIndexMap<MessageId, u64, 64>,
}

impl MessageController {
    pub fn new() -> Self {
        Self::default()
    }

    pub async fn handle_msg_lumen(&mut self, buf: &[u8]) -> Result<(), ()> {
        let mut reader = ByteStreamReader::new(buf);
        let decoded = ControllerMessage::deserialize_from(&mut reader);

        if let Err(e) = decoded {
            error!("error decoding message {}", e);
            return Err(());
        }

        let ControllerMessage { timestamp, kind } = decoded.unwrap();
        let message_id = MessageId::from(&kind);

        let is_new_value = self.update_message_timestamp(message_id, timestamp);
        if !is_new_value {
            return Err(());
        }

        match kind {
            MessageKind::Empty => Ok(()),
            MessageKind::KeepAlive { duration } => {
                set_keep_alive_buffer(duration).await;
                Ok(())
            }
            MessageKind::LedState {
                strip_id: _,
                led_values,
            } => {
                set_led_state_buffer(led_values).await;
                Ok(())
            }
        }
    }

    fn update_message_timestamp(&mut self, message_id: MessageId, timestamp: u64) -> bool {
        let is_new_value = match self.timestamp_map.entry(message_id) {
            heapless::Entry::Occupied(entry) => {
                if *entry.get() < timestamp {
                    entry.insert(timestamp);
                    true
                } else {
                    false
                }
            }
            heapless::Entry::Vacant(entry) => {
                entry.insert(timestamp).unwrap();
                true
            }
        };
        is_new_value
    }
}
