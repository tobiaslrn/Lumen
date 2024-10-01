use defmt::Format;

use super::message_kind::MessageKind;

#[repr(u16)]
#[derive(Clone, Copy, PartialEq, Eq, Hash)]
pub enum MessageId {
    Empty = 0,
    KeepAlive = 1,
    LedState = 2,
}

impl TryFrom<u16> for MessageId {
    type Error = ();

    fn try_from(v: u16) -> Result<Self, Self::Error> {
        match v {
            x if x == MessageId::Empty as u16 => Ok(MessageId::Empty),
            x if x == MessageId::KeepAlive as u16 => Ok(MessageId::KeepAlive),
            x if x == MessageId::LedState as u16 => Ok(MessageId::LedState),
            _ => Err(()),
        }
    }
}

impl From<&MessageKind> for MessageId {
    fn from(value: &MessageKind) -> Self {
        match value {
            MessageKind::Empty => MessageId::Empty,
            MessageKind::KeepAlive { .. } => MessageId::KeepAlive,
            MessageKind::LedState { .. } => MessageId::LedState,
        }
    }
}

impl Format for MessageId {
    fn format(&self, f: defmt::Formatter) {
        match self {
            MessageId::Empty => defmt::write!(f, "Empty"),
            MessageId::KeepAlive => defmt::write!(f, "KeepAlive"),
            MessageId::LedState => defmt::write!(f, "LedState"),
        }
    }
}
