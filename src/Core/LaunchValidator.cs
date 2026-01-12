/* Project Aegis-Link | Tactical C2 Interface | Safety Engineering by Mahdy Gribkov */

namespace AegisLink.Core
{
    public static class LaunchValidator
    {
        public static bool IsSafeToLaunch(TelemetryFrame frame)
        {
            // Senior Engineering Safety Rules:
            // 1. Battery must be > 10%
            // 2. System must be Armed (Bit 0 of StatusFlags)
            // 3. System must be Locked (Bit 1 of StatusFlags)
            
            bool hasBattery = frame.BatteryLevel > 10;
            bool isArmed = (frame.StatusFlags & 0x01) != 0;
            bool isLocked = (frame.StatusFlags & 0x02) != 0;

            return hasBattery && isArmed && isLocked;
        }

        public static string GetSafetyReport(TelemetryFrame frame)
        {
            if (IsSafeToLaunch(frame)) return "READY FOR LAUNCH";

            var issues = new System.Collections.Generic.List<string>();
            if (frame.BatteryLevel <= 10) issues.Add("LOW BATT");
            if ((frame.StatusFlags & 0x01) == 0) issues.Add("UNARMED");
            if ((frame.StatusFlags & 0x02) == 0) issues.Add("NO LOCK");

            return "ABORT: " + string.Join(" | ", issues);
        }
    }
}
