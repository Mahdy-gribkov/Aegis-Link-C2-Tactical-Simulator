namespace AegisLink.Core
{
    public static class TacticalConstants
    {
        // Networking Configuration
        public const int DEFAULT_PORT = 5005;
        public const int HEARTBEAT_INTERVAL_MS = 100;
        public const int WATCHDOG_TIMEOUT_MS = 500;

        // Security Configuration
        public const byte XOR_SHIELD_KEY = 0x42;

        // Protocol Configuration
        public const byte PROTOCOL_VERSION = 0x01;
    }
}
