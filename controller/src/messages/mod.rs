pub mod bytestreamreader;
pub mod message_id;
pub mod message_kind;
pub mod rgb8;

use bytestreamreader::{ByteStreamReader, MessageDeserializer};
use message_kind::MessageKind;

pub type DeserializationResult<T> = Result<T, ()>;

#[derive(Debug)]
pub struct ControllerMessage {
    pub timestamp: Timestamp,
    pub kind: MessageKind,
}

impl MessageDeserializer for ControllerMessage {
    type Result = DeserializationResult<Self>;

    fn deserialize_from(reader: &mut ByteStreamReader) -> Self::Result {
        let timestamp = Timestamp::new(reader.u64());
        let kind = MessageKind::deserialize_from(reader)?;

        Ok(ControllerMessage { timestamp, kind })
    }
}

#[derive(Debug, Clone, Copy, Hash, PartialEq, Eq, PartialOrd, Ord)]
pub struct Timestamp(u64);

impl Timestamp {
    pub fn new(timestamp: u64) -> Self {
        Timestamp(timestamp)
    }

    pub fn get(&self) -> u64 {
        self.0
    }
}

impl From<u64> for Timestamp {
    fn from(timestamp: u64) -> Self {
        Timestamp(timestamp)
    }
}
