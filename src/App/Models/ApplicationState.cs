namespace AegisLink.App.Models;

/// <summary>
/// Application-wide state machine states
/// </summary>
public enum ApplicationState
{
    Idle,       // Standby, no active connection
    Scanning,   // UDP listener active, awaiting packets
    Tracking,   // Receiving live telemetry
    Offline     // Connection lost / error state
}
