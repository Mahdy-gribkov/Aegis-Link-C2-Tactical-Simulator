/* Project Aegis-Link | Tactical C2 Interface | Core Engineering by Mahdy Gribkov */

using System;
using System.Runtime.InteropServices;

namespace AegisLink.Core
{
    public static class ProtocolMapper
    {
        public static TelemetryFrame FromBytes(ReadOnlySpan<byte> data)
        {
            // Efficient zero-copy conversion from raw bytes to struct
            // Assumes data length matches struct size exactly
            return MemoryMarshal.Read<TelemetryFrame>(data);
        }
    }
}
