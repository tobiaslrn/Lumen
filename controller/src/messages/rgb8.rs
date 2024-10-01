use super::{
    bytestreamreader::{ByteStreamReader, MessageDeserializer},
    DeserializationResult,
};

#[derive(Debug, Clone, Copy)]
pub struct Rgb8 {
    pub r: u8,
    pub g: u8,
    pub b: u8,
}

impl Rgb8 {
    pub fn read_from_bsr(bsr: &mut ByteStreamReader) -> Rgb8 {
        let r = bsr.u8();
        let g = bsr.u8();
        let b = bsr.u8();
        Rgb8 { r, g, b }
    }
}

impl MessageDeserializer for Rgb8 {
    type Result = DeserializationResult<Rgb8>;

    fn deserialize_from(reader: &mut ByteStreamReader) -> Self::Result {
        let r = reader.u8();
        let g = reader.u8();
        let b = reader.u8();
        Ok(Rgb8 { r, g, b })
    }
}
