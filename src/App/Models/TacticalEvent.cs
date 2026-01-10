namespace AegisLink.App.Models;

/// <summary>
/// Types of tactical events that can occur
/// </summary>
public enum EventType
{
    Ping,
    Burst,
    KeepAlive,
    Disconnect,
    DataPacket
}

/// <summary>
/// Immutable record representing a tactical event
/// </summary>
public record TacticalEvent(
    DateTime Timestamp,
    EventType Type,
    float SignalStrength,
    string? PayloadData = null
);
