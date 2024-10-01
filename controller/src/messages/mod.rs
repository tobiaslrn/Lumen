pub mod bytestreamreader;
pub mod message_id;
pub mod message_kind;
pub mod rgb8;

use bytestreamreader::{ByteStreamReader, MessageDeserializer};
use message_kind::MessageKind;

pub type DeserializationResult<T> = Result<T, ()>;

#[derive(Debug)]
pub struct ControllerMessage {
    pub timestamp: u64,
    pub kind: MessageKind,
}

impl MessageDeserializer for ControllerMessage {
    type Result = DeserializationResult<Self>;

    fn deserialize_from(reader: &mut ByteStreamReader) -> Self::Result {
        let timestamp = reader.u64();
        let kind = MessageKind::deserialize_from(reader)?;

        Ok(ControllerMessage { timestamp, kind })
    }
}
