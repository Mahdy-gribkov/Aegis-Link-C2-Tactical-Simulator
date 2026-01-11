# Aegis-Link C2 - User Manual

## Terminal Commands

| Command | Description |
|---------|-------------|
| `scan` | Start UDP listener / simulation |
| `stop` | Stop listener |
| `status` | Show current state (AZ, SIG) |
| `export -log` | Save terminal logs to Documents |
| `clear` | Clear terminal output |
| `help` | Show available commands |

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `~` | Toggle Terminal |
| `F11` | Toggle Fullscreen |
| `Esc` | Close Terminal |
| `Ctrl+C` | Copy last 10 terminal logs |
| `Ctrl+V` | Paste into command input |

## Radar Navigation

- **Zoom**: Mouse wheel (0.5x - 4x range)
- **Pan**: Click and drag on radar canvas

## UDP Connection Setup

### Port Configuration
Default: **5555** (configurable in `%LOCALAPPDATA%\AegisLink\config.json`)

### Test Connection (PowerShell)
```powershell
$udp = New-Object System.Net.Sockets.UdpClient
$udp.Connect("127.0.0.1", 5555)
$bytes = [Text.Encoding]::ASCII.GetBytes('{"type":"ping","val":90}')
$udp.Send($bytes, $bytes.Length)
```

### Firewall Rule
```powershell
New-NetFirewallRule -DisplayName "AegisLink UDP" -Direction Inbound -Protocol UDP -LocalPort 5555 -Action Allow
```

## Troubleshooting

### "NO SIGNAL" Status
- Verify UDP packets are being sent to correct IP:Port
- Check Windows Firewall allows UDP 5555
- Run `scan` command to activate listener

### Crash Logs
Location: `%LOCALAPPDATA%\AegisLink\logs\crash.log`
