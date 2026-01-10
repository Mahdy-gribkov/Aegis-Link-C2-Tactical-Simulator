/* Project Aegis-Link | Tactical C2 Interface | Core Engineering by Mahdy Gribkov */

namespace AegisLink.Core
{
    public static class SecurityUtils
    {
        // Shared symmetrical key for the tactical link
        private const byte TacticalKey = 0x42; 

        public static byte[] CalculateXorResponse(byte[] data)
        {
            if (data == null)
            {
                return new byte[0];
            }

            byte[] result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                // Simple XOR cipher
                result[i] = (byte)(data[i] ^ TacticalKey);
            }

            return result;
        }

        public static bool VerifyHandshake(byte[] challenge, byte[] response)
        {
            if (challenge == null || response == null)
            {
                return false;
            }

            if (challenge.Length != response.Length)
            {
                return false;
            }

            for (int i = 0; i < challenge.Length; i++)
            {
                byte expected = (byte)(challenge[i] ^ TacticalKey);

                if (response[i] != expected)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
