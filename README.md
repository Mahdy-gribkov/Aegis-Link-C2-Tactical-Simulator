# AEGIS-LINK | Tactical C2 Interface

![Build Status](https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator/actions/workflows/tactical-ci-cd.yml/badge.svg)

**Aegis-Link** is a defense-grade Command & Control (C2) tactical interface built on .NET 6.0 LTS. It provides secure, low-latency telemetry integration between desktop command stations and M5Stack Cardputer field hardware.

---

## Mission Capabilities

### Real-Time Telemetry Processing
- **Zero-Copy Deserialization:** Binary protocol with `Pack=1` memory alignment for sub-millisecond frame processing
- **Dual-Mode Operation:** Seamless switching between live hardware link and virtual simulation
- **Thread-Safe UI Marshalling:** Asynchronous event streams with `Dispatcher.BeginInvoke` for zero-latency rendering

### Security Architecture
- **Challenge-Response Handshake:** Mandatory XOR-based authentication preventing unauthorized node intrusion
- **Endpoint Locking:** Single-source UDP binding after successful handshake
- **Mission Black Box:** SQLite-based event sourcing for post-mission analysis and audit trails

### Tactical Interface
- **High-Contrast HUD:** Cyan-on-black design optimized for ruggedized laptop displays
- **Scalable Viewbox:** Resolution-independent rendering from 1024x768 to 4K
- **Monospace Telemetry:** Consolas font family for precise numerical readability

---

## Hardware Requirements

### Command Station (PC)
- **OS:** Windows 10/11 (64-bit)
- **Runtime:** .NET 6.0 SDK or self-contained deployment
- **Network:** UDP port 5000 (configurable)
- **Display:** Minimum 1024x768, recommended 1920x1080+

### Field Hardware (M5Stack Cardputer)
- **MCU:** ESP32-S3
- **Connectivity:** WiFi 802.11 b/g/n
- **Firmware:** Custom C++ implementation (see `Hardware/main.cpp`)
- **Power:** USB-C or battery (3.7V LiPo)

---

## Quick Start

### Self-Contained Deployment
1. Download the latest release from [GitHub Actions Artifacts](https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator/actions)
2. Extract `AegisLink-Tactical-Release.zip`
3. Run `AegisLink.App.exe` (no installation required)

### Build from Source
```powershell
git clone https://github.com/Mahdy-gribkov/Aegis-Link-C2-Tactical-Simulator.git
cd Aegis-Link-C2-Tactical-Simulator
dotnet restore
dotnet build --configuration Release
dotnet run --project App/AegisLink.App.csproj
```

### Run Tests
```powershell
dotnet test --configuration Release
```

---

## Security Architecture

### Binary Protocol Determinism
The `TelemetryFrame` struct uses `StructLayout(LayoutKind.Sequential, Pack=1)` to ensure bit-for-bit compatibility between .NET (C#) and embedded C++ implementations. This eliminates JSON overhead and enables zero-copy memory mapping.

### XOR Challenge-Response
1. PC sends random 8-bit challenge to Cardputer
2. Cardputer responds with `challenge XOR 0x42`
3. PC verifies response and locks endpoint
4. All subsequent telemetry frames validated against locked source

See [SECURITY.md](docs/SECURITY.md) for detailed protocol specification.

---

## Documentation

- **[Design Rationale](docs/DESIGN_RATIONALE.md)** - Architectural decisions and engineering trade-offs
- **[Security Protocol](docs/SECURITY.md)** - XOR handshake and endpoint locking
- **[Architecture Guide](docs/ARCHITECTURE.md)** - Binary determinism, async patterns, MVVM implementation
- **[Dependency Manifest](DEPENDENCY_MANIFEST.md)** - Complete dependency list with licenses

---

## Maintainer

**Mahdy Gribkov**  
Core Engineering & Systems Architecture  
[GitHub](https://github.com/Mahdy-gribkov)

---

## License

This project is provided for educational and tactical simulation purposes. All dependencies are licensed under permissive open-source licenses (MIT, Apache 2.0). See [DEPENDENCY_MANIFEST.md](DEPENDENCY_MANIFEST.md) for complete license information.

---

*AEGIS-LINK | Defense-Grade Tactical Awareness*
