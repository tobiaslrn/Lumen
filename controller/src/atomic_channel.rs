use core::cell::Cell;
use embassy_sync::blocking_mutex::raw::RawMutex;
use embassy_sync::mutex::Mutex;

/// A channel that holds only one item at a time.
#[derive(Default)]
pub struct AtomicChannel<M, T>
where
    M: RawMutex,
{
    inner: Mutex<M, Cell<Option<T>>>,
}

impl<M, T> AtomicChannel<M, T>
where
    M: RawMutex,
{
    /// Creates a new empty channel.
    pub const fn new() -> Self {
        Self {
            inner: Mutex::new(Cell::new(None)),
        }
    }

    /// Sends a value into the channel, overwriting the current value if it exists.
    pub async fn send(&self, message: T) {
        self.inner.lock().await.set(Some(message))
    }

    /// Receives the value from the channel, returning `None` if it is empty.
    pub async fn recv(&self) -> Option<T> {
        self.inner.lock().await.take()
    }

    /// Receives the value from the channel, blocking until it is available.
    pub async fn recv_item(&self) -> T {
        loop {
            if let Some(item) = self.recv().await {
                return item;
            }
        }
    }
}
