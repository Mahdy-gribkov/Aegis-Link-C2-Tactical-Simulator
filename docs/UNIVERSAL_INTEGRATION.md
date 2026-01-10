# Universal Integration Protocol

## Overview

Aegis-Link implements a **platform-agnostic binary protocol** that enables any device to act as a tactical telemetry source. While the reference implementation uses M5Stack Cardputer, the protocol is designed for universal adoption across:

- **Embedded Systems:** ESP32, Arduino, STM32, Raspberry Pi
- **Mobile Platforms:** iOS (Swift/Objective-C), Android (Kotlin/Java)
- **Desktop Systems:** Linux, macOS, Windows
- **Custom Hardware:** FPGA, DSP, specialized sensors

---

## Protocol Specification

### Transport Layer
| Parameter | Value |
|-----------|-------|
| Protocol | UDP |
| Default Port | 5000 |
| Byte Order | Little-Endian |
| Frame Rate | 10 Hz (recommended) |

### Authentication Handshake

Before transmitting telemetry, the launcher must complete challenge-response authentication:

```
Step 1: PC → Launcher: [CHALLENGE_BYTE] (1 byte, random 0x00-0xFF)
Step 2: Launcher → PC: [RESPONSE_BYTE]  (1 byte, CHALLENGE XOR 0x42)
Step 3: PC validates response and locks endpoint
```

---

## TelemetryFrame Binary Structure

### Frame Layout (28 bytes total)

```
Offset  Size    Type      Field           Description
──────  ────    ────      ─────           ───────────
0x00    4       int32     BatteryLevel    Battery percentage (0-100)
0x04    4       float32   SignalStrength  Signal strength in dB
0x08    8       float64   Latitude        GPS latitude (degrees)
0x10    8       float64   Longitude       GPS longitude (degrees)
0x18    4       uint32    StatusCodes     Bitfield status flags
──────  ────
Total:  28 bytes
```

### Hex Dump Example

```
00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F
── ── ── ── ── ── ── ── ── ── ── ── ── ── ── ──
4B 00 00 00 00 00 48 C2 5C 8F C2 F5 28 5C 28 40
│           │           │                       
│           │           └── Latitude: 12.34°
│           └── SignalStrength: -50.0 dB
└── BatteryLevel: 75%

52 B8 1E 85 EB 63 4C 40 01 00 00 00
│                       │
│                       └── StatusCodes: 0x01 (NOMINAL)
└── Longitude: 56.78°
```

---

## Implementation Examples

### C/C++ (Embedded)
```c
#pragma pack(push, 1)
typedef struct {
    int32_t  BatteryLevel;
    float    SignalStrength;
    double   Latitude;
    double   Longitude;
    uint32_t StatusCodes;
} TelemetryFrame;
#pragma pack(pop)

void send_telemetry(int sock, struct sockaddr_in* addr) {
    TelemetryFrame frame = {
        .BatteryLevel = 75,
        .SignalStrength = -50.0f,
        .Latitude = 12.34,
        .Longitude = 56.78,
        .StatusCodes = 0x01
    };
    sendto(sock, &frame, sizeof(frame), 0, 
           (struct sockaddr*)addr, sizeof(*addr));
}
```

### Python (Desktop/Mobile)
```python
import struct
import socket

FRAME_FORMAT = '<ifddI'  # Little-endian: int, float, double, double, uint

def send_telemetry(sock, addr):
    frame = struct.pack(FRAME_FORMAT,
        75,      # BatteryLevel
        -50.0,   # SignalStrength
        12.34,   # Latitude
        56.78,   # Longitude
        0x01     # StatusCodes
    )
    sock.sendto(frame, addr)
```

### Swift (iOS)
```swift
struct TelemetryFrame {
    var batteryLevel: Int32
    var signalStrength: Float
    var latitude: Double
    var longitude: Double
    var statusCodes: UInt32
}

func sendTelemetry(socket: GCDAsyncUdpSocket, address: Data) {
    var frame = TelemetryFrame(
        batteryLevel: 75,
        signalStrength: -50.0,
        latitude: 12.34,
        longitude: 56.78,
        statusCodes: 0x01
    )
    let data = Data(bytes: &frame, count: MemoryLayout<TelemetryFrame>.size)
    socket.send(data, toAddress: address, withTimeout: -1, tag: 0)
}
```

---

## Status Codes (Bitfield)

| Bit | Hex | Constant | Description |
|-----|-----|----------|-------------|
| 0 | 0x01 | STATUS_NOMINAL | System operating normally |
| 1 | 0x02 | STATUS_LOW_BATTERY | Battery below 20% |
| 2 | 0x04 | STATUS_GPS_LOCK | GPS has valid fix |
| 3 | 0x08 | STATUS_SENSOR_ERROR | Sensor malfunction |
| 4 | 0x10 | STATUS_COMMS_ACTIVE | Active communication |

---

## Validation Requirements

For a device to be Aegis-Link compatible:

1. ✅ Must transmit exactly 28 bytes per frame
2. ✅ Must use Little-Endian byte order
3. ✅ Must use `#pragma pack(1)` or equivalent (no padding)
4. ✅ Must complete XOR handshake before telemetry
5. ✅ Must respond within 1000ms timeout

---

*Lead Systems Architect: Mahdy Gribkov*
