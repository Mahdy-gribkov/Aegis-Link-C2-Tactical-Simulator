# Tactical Memory Strategy: Aegis-Link C2
**Classification:** UNCLASSIFIED // ENGINEERING REPORT
**Author:** Mahdy Gribkov

## Overview
Aegis-Link v3.0 uses a high-performance memory strategy optimized for real-time tactical C2 operations. The primary goal is minimizing Garbage Collector (GC) latency during high-frequency telemetry processing (60Hz+).

## Structs vs. Classes (Stack vs. Heap)
The `TelemetryFrame` is implemented as a `readonly struct`.

### Architectural Choice: The Stack
- **Zero GC Overhead**: Structs are value types stored on the stack (when local) or inlined within their parent object. They do not require heap allocation or garbage collection.
- **Data Locality**: By using a 6-byte packed struct, data is compact and cache-friendly.
- **Safety**: Being `readonly` prevents accidental state mutation during flight.

## Command Collection Synchronization
High-speed terminal logging from background networking threads traditionally requires expensive `Dispatcher.Invoke` calls, which can cause UI stuttering.

### Strategy: Thread-Safe Synchronization
We use `BindingOperations.EnableCollectionSynchronization` on the `TerminalLog` collection. This allows background radio threads to directly append data to the UI-bound list without thread affiliation errors, significantly reducing CPU context-switching overhead.

## Resource Lifecycle
All unmanaged networking resources (UDP sockets) implement the `IDisposable` pattern to ensure deterministic cleanup and prevent memory leaks in long-running tactical sessions.
