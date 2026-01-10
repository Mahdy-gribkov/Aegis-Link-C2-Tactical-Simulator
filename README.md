# Aegis-Link C2 Tactical Simulator

**Aegis-Link** is a high-performance C2 (Command & Control) Tactical Simulator built on .NET 6 WPF. It provides a secure, low-latency bridge between a desktop dashboard and the M5Stack Cardputer.

## 🚀 Quick Start (Deployment Build)

If you have the **Tactical Release** package:
1.  Locate `AegisLink.App.exe` in the `publish` directory.
2.  Launch the application (No .NET installation required, self-contained).
3.  Toggle **[SIMULATION MODE]** to verify UI behavior immediately.

## 🛠️ Build from Source

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator.git
    cd Aegis-Link-C2-Tactical-Simulator
    ```
2.  **Build the Solution**:
    ```powershell
    dotnet build
    ```
3.  **Run with Tests**:
    ```powershell
    dotnet test
    dotnet run --project App/AegisLink.App.csproj
    ```

## 🎯 Tactical Features

- **Binary Determinism**: Zero-copy deserialization using `Pack=1` memory alignment for sub-millisecond telemetry processing.
- **Secure Link**: Mandatory Challenge-Response XOR Handshake preventing unauthorized node intrusion.
- **Mission Persistence**: SQLite/Dapper "Black Box" logging for every command and connection event.
- **Aero-Space UI**: Custom-built MVVM dashboard with high-visibility dark theme and thread-safe data marshalling.
- **Hardware Agnostic**: Integrated simulation layer allowing mission validation without physical hardware nearby.

## 📄 Documentation
- [Design Rationale](docs/DESIGN_RATIONALE.md) - **Recommended for Seniors**
- [Security Protocol](docs/SECURITY.md)
- [Architecture & Design](docs/ARCHITECTURE.md)

---
*Created for the Aegis-Link Project. Defense-Grade Tactical Awareness.*
