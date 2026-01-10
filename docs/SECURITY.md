# Aegis-Link Security Protocol

## The "Barrier to Entry" Handshake
To prevent unauthorized hardware or stray UDP packets from confusing the Tactical C2 Simulator, we implement a strict **Application Layer Handshake**.

### The Why
- **UDP is Connectionless**: The `UdpClient` will happily accept packets from *any* IP address on the listening port.
- **Hardware Authentication**: We need to verify that the sender is a trusted "Aegis-Link" node (e.g., M5 Cardputer) and not a random network scanner.

### The Protocol
1.  **Beacon (Device -> PC)**: The Cardputer sends any data packet to the PC's IP port 5005.
2.  **Challenge (PC -> Device)**: The PC, seeing an unauthenticated endpoint, generates a **Random Challenge Byte (0-255)** and sends it back to the originating IP.
3.  **Calculation (Device)**: The Cardputer receives the byte and performs an XOR operation: `Response = Challenge ^ TACTICAL_KEY (0x42)`.
4.  **Response (Device -> PC)**: The Cardputer sends the single-byte response back.
5.  **Verification (PC)**: The PC checks if `ReceivedResponse == (StoredChallenge ^ 0x42)`.
6.  **Trust**: If valid, the PC "Locks" onto that IP Endpoint. All subsequent packets from this IP are parsed as `TelemetryFrame`. Packets from other IPs are ignored or challenged.

### Binary Security
- **XOR Cipher**: While simple, it prevents accidental connections and basic replay attacks if the challenge is random.
- **Endpoint Locking**: Prevents "Man-in-the-Middle" injection from other IPs once the session is established.
