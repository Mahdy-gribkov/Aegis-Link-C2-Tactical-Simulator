# Aegis-Link Architecture

## The Why

### Memory Management: Stack vs. Heap
In high-frequency tactical simulation and C2 (Command & Control) systems, latency and predictability are paramount. We represent the `TelemetryFrame` as a **readonly struct** (Value Type) rather than a class (Reference Type).

**Why?**
- **Stack Allocation**: Value types are allocated on the Stack (or inline in parent types). This avoids Heap allocation for every single incoming packet.
- **GC Pressure**: By avoiding Heap allocations, we significantly reduce the work required by the Garbage Collector (GC). In a scenario where packets arrive continuously, creating a new `class` instance for each packet would cause "GC Pauses," leading to jitter in the simulator.
- **Performance**: Stack access is immediate and localized in CPU cache.

### Asynchronous Networking
We utilize the **Task-based Asynchronous Pattern (TAP)** with `UdpClient`.
- **Non-Blocking I/O**: The listener runs on a background thread (`Task.Run`) to ensure the main UI thread is never blocked waiting for network packets.
- **Responsiveness**: This keeps the WPF application responsive (e.g., capable of rendering 60 FPS animations) even during heavy network traffic.

### Thread-Safe Disposal Pattern
To properly clean up the background listener without causing "socket hangs" or race conditions, we employ a rigorous disposal pattern in `UdpLinkService`:
1.  **CancellationToken**: We signal the `CancellationTokenSource` first. This alerts the loop to stop.
2.  **Socket Closure**: We proactively call `_udpClient.Close()`. This forces any pending `ReceiveAsync` operation to throw an exception immediately (caught gracefully), unblocking the thread.
3.  **Task Wait**: We (optionally with timeout) wait for the background task to complete its cleanup block. This ensures deterministic shutdown—we know 100% the thread is dead before the application closes.

### MVVM Architecture & Thread Marshalling

#### The Dispatcher
The `UdpLinkService` receives data on a background thread pool thread. WPF controls (like `TextBlock`) have thread affinity and can **only** be updated by the main UI thread.
- **Problem**: Directly updating `MainViewModel` properties from the UDP event handler would throw an `InvalidOperationException`.
- **Solution**: We use `Application.Current.Dispatcher.Invoke(...)`. This marshals the execution delegate onto the UI thread queue, ensuring "Thread Affinity" rules are respected.

#### Observer Pattern (INotifyPropertyChanged)
We implemented a custom `ViewModelBase` without external libraries.
- **Mechanism**: The ViewModel acts as the "Subject" and the WPF View (via Data Binding) acts as the "Observer."
- **CallerMemberName**: We use this compiler attribute to avoid hardcoding property strings ("Azimuth"), making refactoring safer.

#### Thread-Safe Collections
For the `CommandLog`, we use `ObservableCollection<T>`. However, modifying this collection from a background thread (even with `Dispatcher` usage for other things) can be tricky if multiple threads read it (e.g., the UI rendering logic).
- **BindingOperations.EnableCollectionSynchronization**: We explicitly register a lock object with WPF's binding engine. This tells WPF to acquire this lock whenever it reads the collection on the UI thread, allowing us to safely modify it from background threads (wrapping modifications in the same lock or relying on the synchronization context).

### OOP Principles
- **Encapsulation**: `SecurityUtils` encapsulates the XOR logic, preventing "magic numbers" or loose bitwise operations from scattering throughout the codebase.
- **Immutability**: `TelemetryFrame` is immutable (`readonly struct`), making it inherently thread-safe when passed between the network thread and the UI thread.

### Binary Determinism & Memory Alignment
To ensure the Aegis-Link C2 system (PC/.NET) correctly interprets raw bytes from the M5 Cardputer (C++/embedded), we strictly define the memory layout of our data structures.

- **StructLayout(LayoutKind.Sequential)**: Forces the runtime to arrange fields in the order defined, matching the C-struct definition on the firmware side.
- **Pack = 1**: Disables default field padding. By default, compilers align fields to 4 or 8 byte boundaries for CPU efficiency. However, raw radio packets are often tightly packed. `Pack = 1` ensures byte-for-byte alignment between sender and receiver.
- **Endianness**: We observe that both the ESP32-S3 (Cardputer) and standard x86/x64 Windows PCs are **Little Endian**. Thus, we can cast raw bytes directly to the struct without reordering. If multi-platform support (e.g., Big Endian servers) were added, we would need to implement an Endianness check and byte-swap procedure in `ProtocolMapper`.

### Zero-Copy Serialization
We use `System.Runtime.InteropServices.MemoryMarshal` to reinterpret raw network bytes (`byte[]` or `Span<byte>`) directly into a `TelemetryFrame` struct. This avoids the CPU overhead of parsing fields individually (e.g., `BitConverter.ToInt32(...)`), resulting in nanosecond-level deserialization suitable for high-frequency telemetry.
