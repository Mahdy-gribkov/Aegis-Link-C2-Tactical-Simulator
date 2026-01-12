# C2 Tactical Protocol Specification [UNCLASSIFIED]
**Version:** 3.0.0 [BIT-PACKED]
**Author:** Mahdy Gribkov

## Overview
To minimize latency and simulate real-world low-bandwidth tactical radio links, Aegis-Link v3.0 uses a 6-byte bit-packed binary protocol.

## Protocol Layout (7 Bytes)

| Byte | Field | Type | Scale/Mask |
|------|-------|------|------------|
| 0 | **Version** | `Byte` | 0x01 (SSOT) |
| 1-2 | **Azimuth** | `Int16` | 1:100 (short = float * 100) |
| 3-4 | **Elevation** | `Int16` | 1:100 (short = float * 100) |
| 5 | **Status Flags** | `Byte` | Bit 0: ARMED, Bit 1: LOCKED |
| 6 | **Battery** | `Byte` | 0-100 (Unsigned Byte) |

## Binary Mapping
```
[VV] [AA][AA] [EE][EE] [SS] [BB]
 |    |  |     |  |     |    |
 |    |  |     |  |     |    +-- Battery (0-100)
 |    |  |     |  |     +------- Flags (000000LS)
 |    |  |     +--+------------- Elevation (Little Endian)
 |    +--+---------------------- Azimuth (Little Endian)
 +------------------------------ Protocol Version
```

## Security Handshake
1. PC sends 1-byte random challenge.
2. Hardware applies XOR `0x42` and returns response.
3. Pulse-mode telemetry commences only on validation.
