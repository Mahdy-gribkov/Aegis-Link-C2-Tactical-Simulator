# Aegis-Link Design Rationale

## Binary Layout Determinism
To bridge the gap between High-Level Managed Code (.NET) and Low-Level Embedded Code (C++), we utilize **LayoutKind.Sequential** with **Pack=1**. This ensures that the memory signature of our `TelemetryFrame` is bit-for-bit identical across both platforms, eliminating the need for expensive JSON serialization.

## Asynchronous Event Sourcing
The tactical link operates as an asynchronous event stream. Telemetry arrives on a background UDP thread and is dispatched to the UI layer via the **Dispatcher Marshalling** pattern. This ensures that the primary UI thread is never blocked by I/O, maintaining the "Aero-Space" responsiveness of the dashboard.

## Decoupled Persistence
Mission data is persistend using a decoupled **Repository Pattern** with **SQLite** and **Dapper**. This "Black Box" logging provides post-mission analysis capabilities while strictly separating I/O concerns from the business logic of the ViewModels.

## Interface-Based Link Abstraction
The `IDataLink` interface allows for seamless transition between **Live Hardware** and **Virtual Simulation**. This strategy facilitates unit testing and allows for software validation in environments where physical hardware is unavailable.

## Custom MVVM Foundation
By implementing a custom `ViewModelBase` and `RelayCommand`, the system maintains a zero-dependency footprint on external frameworks like Prism or CommunityToolkit. This ensures maximum portability and demonstrates a deep understanding of the MVVM pattern and the **INotifyPropertyChanged** mechanism.

## Automated Quality Assurance & Deployment
To guarantee mission integrity, Aegis-Link employs a **GitHub Actions CI/CD Pipeline**. Every commit triggers a full suite of unit tests verifying the binary protocol and security handshake. The system targets **.NET 6.0 (LTS)** to ensure maximum compatibility with ruggedized field hardware and long-term maintenance cycles. Upon a successful merge to `main`, a **Self-Contained Release Candidate** is automatically published as a GitHub Artifact, ensuring that a "Flight-Ready" build is always accessible for deployment to ruggedized hardware without external runtime dependencies.

## Cross-Platform Build Constraints
The Aegis-Link C2 dashboard is built using **WPF (Windows Presentation Foundation)**. Due to the deep integration with the Windows Desktop Stack required for high-performance tactical rendering, a **Windows Runner** (`windows-latest`) is mandatory in the CI/CD pipeline. This ensures that the build environment has access to the necessary desktop SDKs and MSBuild components that are not available on Linux or macOS runners.
