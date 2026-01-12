# C2 Tactical Protocol Specification [UNCLASSIFIED]
**Version:** 3.0.0 [BIT-PACKED]
**Author:** Mahdy Gribkov

## Overview
To minimize latency and simulate real-world low-bandwidth tactical radio links, Aegis-Link v3.0 uses a 6-byte bit-packed binary protocol.

## Protocol Layout (6 Bytes)

| Byte | Field | Type | Scale/Mask |
|------|-------|------|------------|
| 0-1 | **Azimuth** | `Int16` | 1:100 (short = float * 100) |
| 2-3 | **Elevation** | `Int16` | 1:100 (short = float * 100) |
| 4 | **Status Flags** | `Byte` | Bit 0: ARMED, Bit 1: LOCKED |
| 5 | **Battery** | `Byte` | 0-100 (Unsigned Byte) |

## Binary Mapping
```
[AA][AA] [EE][EE] [SS] [BB]
 |  |     |  |     |    |
 |  |     |  |     |    +-- Battery (0-100)
 |  |     |  |     +------- Flags (000000LS)
 |  |     +--+------------- Elevation (Little Endian)
 +--+---------------------- Azimuth (Little Endian)
```

## Security Handshake
1. PC sends 1-byte random challenge.
2. Hardware applies XOR `0x42` and returns response.
3. Pulse-mode telemetry commences only on validation.
