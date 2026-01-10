# Aegis-Link Interview Talking Points

## 1. UDP vs. TCP (Latency is King)
> "Why did you choose UDP for telemetry?"
*   **Speed**: TCP handshake and acknowledgments introduce overhead. In a tactical scenario, if a packet is dropped, we don't want to pause everything to retransmit old data. We want the *next* fresh packet immediately.
*   **Fire-and-Forget**: For telemetry (battery, GPS), 60Hz updates mean missing one frame is irrelevant. UDP allows for the lowest possible latency between the hardware and the UI.

## 2. Structs vs. Classes (Memory Pressure)
> "Why use a struct for TelemetryFrame?"
*   **Stack Allocation**: `TelemetryFrame` is a `readonly struct`. This means it lives on the Stack (or register), not the Heap.
*   **Zero GC**: If we used a Class, processing 60 packets/sec would create 3,600 objects/minute that the Garbage Collector eventually has to clean up. This causes "GC Pauses" (stutter). Structs eliminate this pressure entirely.

## 3. Dispatcher.Invoke (Thread Safety)
> "How do you handle multi-threading?"
*   **Context**: The UDP Listener runs on a generic background thread (Thread Pool). WPF UI controls have "Thread Affinity" and can only be touched by the Main Thread.
*   **Marshalling**: I use `Application.Current.Dispatcher.Invoke` to force the UI update logic onto the correct thread. This prevents `InvalidOperationException` and keeps the background listener free to keep receiving packets without blocking.

## 4. Bitwise XOR Handshake (Efficient Security)
> "Is the security robust?"
*   **Low Overhead**: We don't need SSL/TLS overhead for a short-range radio link. XOR is a single CPU instruction using a pre-shared key.
*   **Effectiveness**: It effectively acts as a whitelist. Random scanners or malformed packets won't pass the challenge check, ensuring the C2 dashboard only accepts data from the verified Tactical Unit.

## 5. Interface-Based Design (Testability)
> "How did you test this without the hardware?"
*   **IDataLink**: By abstracting the network layer behind an interface, I could swap the real `UdpLinkService` for a `VirtualLauncherService`.
*   **Mocking**: This allowed me to verify the UI, data binding, and battery logic purely in software before the hardware was even ready. It proves a commitment to Test-Driven Development (TDD) principles.
