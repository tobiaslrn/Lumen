use crate::messages::message_id::MessageId;
use crate::messages::message_kind::MessageKind;
use crate::messages::ControllerMessage;
use crate::messages::Timestamp;
use crate::ATOM_KEEP_ALIVE;
use crate::ATOM_LED_STATE;
use defmt::warn;
use heapless::FnvIndexMap;

#[derive(Clone, Default)]
pub struct MessageController {
    message_timestamp_map: FnvIndexMap<MessageId, Timestamp, 64>,
}

impl MessageController {
    pub fn new() -> Self {
        Self::default()
    }

    /// Handles the application logic for the received message.
    /// The message is only processed if the received message is newer than the last one.
    pub async fn handle_msg_lumen(
        &mut self,
        ControllerMessage { timestamp, kind }: ControllerMessage,
    ) {
        let message_id = MessageId::from(&kind);
        let is_new_value = self.update_message_timestamp(message_id, timestamp);
        if !is_new_value {
            warn!("Discarding old message {:?}", message_id);
            return;
        }

        match kind {
            MessageKind::Empty => {}
            MessageKind::KeepAlive { duration } => ATOM_KEEP_ALIVE.send(duration).await,
            MessageKind::LedState {
                strip_id: _,
                led_values,
            } => ATOM_LED_STATE.send(led_values).await,
        }
    }

    /// Updates the timestamp of a message if the new timestamp is greater than the current one.
    /// Returns true if the value was updated.
    fn update_message_timestamp(&mut self, message_id: MessageId, timestamp: Timestamp) -> bool {
        let is_new_value = match self.message_timestamp_map.entry(message_id) {
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
