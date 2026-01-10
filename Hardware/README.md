# Hardware Integration | M5Stack Cardputer

## Overview

The **Hardware** directory contains the embedded firmware for the M5Stack Cardputer field unit. This ESP32-S3 based device serves as the tactical telemetry source, transmitting sensor data to the Aegis-Link C2 desktop interface via UDP.

---

## Hardware Specifications

| Component | Specification |
|-----------|---------------|
| **MCU** | ESP32-S3 (Dual-Core Xtensa LX7) |
| **Connectivity** | WiFi 802.11 b/g/n |
| **Display** | 1.14" IPS LCD (135x240) |
| **Input** | Full QWERTY keyboard |
| **Power** | USB-C / 3.7V LiPo battery |

---

## Communication Protocol

### UDP Telemetry Stream
- **Port:** 5000 (configurable)
- **Frame Size:** 28 bytes
- **Frame Rate:** 10 Hz (configurable)

### Binary Frame Structure
```cpp
struct TelemetryFrame {
    int32_t  BatteryLevel;    // 4 bytes
    float    SignalStrength;  // 4 bytes
    double   Latitude;        // 8 bytes
    double   Longitude;       // 8 bytes
    uint32_t StatusCodes;     // 4 bytes
};                            // Total: 28 bytes (Pack=1)
```

---

## Security Handshake

Before telemetry transmission begins, the Cardputer must complete a challenge-response authentication:

1. **Challenge:** PC sends random 8-bit value
2. **Response:** Cardputer responds with `challenge XOR 0x42`
3. **Lock:** PC validates response and locks endpoint

---

## Firmware Build

### Prerequisites
- [PlatformIO](https://platformio.org/) or Arduino IDE
- ESP32 board support package
- M5Stack Cardputer library

### Build Commands
```bash
cd Hardware
pio run
pio upload
```

---

## File Structure

```
Hardware/
├── main.cpp          # Primary firmware implementation
└── README.md         # This documentation
```

---

*Lead Systems Architect: Mahdy Gribkov*
