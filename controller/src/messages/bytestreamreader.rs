use byteorder::{ByteOrder, LittleEndian};

pub struct ByteStreamReader<'slc> {
    stream: &'slc [u8],
}

impl<'slc> ByteStreamReader<'slc> {
    pub fn new(stream: &'slc [u8]) -> Self {
        Self { stream }
    }

    pub fn u8(&mut self) -> u8 {
        let byte = self.stream[0];
        self.advance_by(1);
        byte
    }

    pub fn u16(&mut self) -> u16 {
        let value = LittleEndian::read_u16(self.stream);
        self.advance_by(size_of::<u16>());
        value
    }

    pub fn u32(&mut self) -> u32 {
        let value = LittleEndian::read_u32(self.stream);
        self.advance_by(size_of::<u32>());
        value
    }

    pub fn u64(&mut self) -> u64 {
        let value = LittleEndian::read_u64(self.stream);
        self.advance_by(size_of::<u64>());
        value
    }

    pub fn advance_by(&mut self, by: usize) {
        self.stream = &self.stream[by..]
    }
}

pub trait MessageDeserializer {
    type Result;

    fn deserialize_from(reader: &mut ByteStreamReader) -> Self::Result;
}
