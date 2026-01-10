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

## Step-by-Step Deployment Guide

### Step 1: Install Development Environment

1. **Download** [Visual Studio Code](https://code.visualstudio.com/)
2. **Install** the PlatformIO IDE extension:
   - Open VS Code → Extensions (Ctrl+Shift+X)
   - Search "PlatformIO IDE"
   - Click Install
3. **Restart** VS Code

### Step 2: Open Project

1. In VS Code, click **File → Open Folder**
2. Navigate to `Aegis-Link-C2-Tactical-Simulator/Hardware/`
3. Click **Select Folder**
4. Wait for PlatformIO to initialize (status bar shows progress)

### Step 3: Configure WiFi Credentials

1. Open `main.cpp`
2. Locate the WiFi configuration section:
   ```cpp
   const char* WIFI_SSID = "AEGIS-LINK";
   const char* WIFI_PASS = "your-password-here";
   const char* TARGET_IP = "192.168.4.1";
   ```
3. Modify as needed for your network

### Step 4: Flash the Cardputer

1. Connect M5Stack Cardputer via USB-C
2. Click the **PlatformIO: Upload** button (→ icon in status bar)
3. Or run from terminal:
   ```powershell
   cd Hardware
   pio run --target upload
   ```
4. Wait for "SUCCESS" message

### Step 5: Connect Command Station

1. On your Windows PC, open **WiFi Settings**
2. Connect to network: `AEGIS-LINK`
3. Enter the password configured in Step 3
4. Launch `AegisLink.App.exe`
5. Verify connection indicator shows **◉ OPERATIONAL**

---

## Troubleshooting

| Issue | Solution |
|-------|----------|
| **Upload fails** | Hold BOOT button while connecting USB |
| **No WiFi network** | Check Cardputer display for AP status |
| **No telemetry** | Verify IP address matches in main.cpp |
| **Handshake fails** | Check XOR key matches in both PC and firmware |

---

## Binary Protocol Reference

### TelemetryFrame Structure (28 bytes)
```cpp
struct TelemetryFrame {
    int32_t  BatteryLevel;    // Offset 0,  4 bytes
    float    SignalStrength;  // Offset 4,  4 bytes
    double   Latitude;        // Offset 8,  8 bytes
    double   Longitude;       // Offset 16, 8 bytes
    uint32_t StatusCodes;     // Offset 24, 4 bytes
};
```

### Handshake Sequence
```
PC → Cardputer: [CHALLENGE_BYTE]
Cardputer → PC: [CHALLENGE_BYTE XOR 0x42]
PC: Validates response, locks endpoint
```

---

## File Structure

```
Hardware/
├── main.cpp          # Firmware implementation
├── platformio.ini    # Build configuration
└── README.md         # This documentation
```

---

*Lead Systems Architect: Mahdy Gribkov*
