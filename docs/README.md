# Aegis-Link C2 Tactical Simulator

**Aegis-Link** is a high-performance C2 (Command & Control) Tactical Simulator built on .NET 6 WPF. It is designed to interface with the M5Stack Cardputer via a secure, binary-aligned UDP protocol.

## Repository
[https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator](https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator)

## Architecture
- **Core**: Generic `IDataLink` interface, `readonly struct` TelemetryFrame with `Pack=1` alignment.
- **Hardware**: C++ Firmware for ESP32-S3 (M5 Cardputer).
- **Security**: Challenge-Response XOR Handshake.
- **UI**: Custom MVVM (No libraries), Aero-Space Modern Dark Theme.

## Build Instructions (CLI)

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator.git
    cd Aegis-Link-C2-Tactical-Simulator
    ```

2.  **Build the Solution**:
    ```powershell
    dotnet build
    ```

3.  **Run the App**:
    ```powershell
    dotnet run --project App/AegisLink.App.csproj
    ```

## Documentation
- [Architecture & Design](docs/ARCHITECTURE.md)
- [Security Protocol](docs/SECURITY.md)
