using System.Runtime.InteropServices;

namespace AegisLink.Core
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public readonly struct TelemetryFrame
    {
        // Explicit fields ensure binary layout matches declaration order exactly
        public readonly int BatteryLevel;
        public readonly float SignalStrength;
        public readonly double Latitude;
        public readonly double Longitude;
        public readonly uint StatusCodes;

        public TelemetryFrame(int batteryLevel, float signalStrength, double latitude, double longitude, uint statusCodes)
        {
            BatteryLevel = batteryLevel;
            SignalStrength = signalStrength;
            Latitude = latitude;
            Longitude = longitude;
            StatusCodes = statusCodes;
        }

        public override string ToString()
        {
            return $"Bat:{BatteryLevel}% Sig:{SignalStrength}dB Lat:{Latitude} Lon:{Longitude} Sts:{StatusCodes}";
        }
    }
}
