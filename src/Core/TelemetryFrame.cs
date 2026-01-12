/* Project Aegis-Link | Tactical C2 Interface | Core Engineering by Mahdy Gribkov */

using System.Runtime.InteropServices;

namespace AegisLink.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TelemetryFrame
    {
        // 6-byte bit-packed protocol for low-bandwidth tactical links
        public readonly short AzimuthScaled;   // float * 100
        public readonly short ElevationScaled; // float * 100
        public readonly byte StatusFlags;      // Bit 0: Armed, Bit 1: Locked
        public readonly byte BatteryLevel;     // 0-100

        public TelemetryFrame(float azimuth, float elevation, bool isArmed, bool isLocked, byte battery)
        {
            AzimuthScaled = (short)(azimuth * 100);
            ElevationScaled = (short)(elevation * 100);
            StatusFlags = (byte)((isArmed ? 0x01 : 0x00) | (isLocked ? 0x02 : 0x00));
            BatteryLevel = battery;
        }

        // Helper properties for UI binding
        public float Azimuth => AzimuthScaled / 100f;
        public float Elevation => ElevationScaled / 100f;
        public bool IsArmed => (StatusFlags & 0x01) != 0;
        public bool IsLocked => (StatusFlags & 0x02) != 0;

        public override string ToString()
        {
            return $"AZ:{Azimuth:F2} EL:{Elevation:F2} Bat:{BatteryLevel}% Flags:{StatusFlags:X2}";
        }
    }
}
