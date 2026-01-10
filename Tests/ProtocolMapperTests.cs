using System;
using System.Runtime.InteropServices;
using AegisLink.Core;
using Xunit;

namespace AegisLink.Tests
{
    public class ProtocolMapperTests
    {
        [Fact]
        public void FromBytes_ShouldMapCorrectly_WhenValidDataProvided()
        {
            // Arrange
            // 28 bytes total for Pack=1 (int, float, double, double, uint)
            // BatteryLevel (int) = 75 (0x4B 00 00 00)
            // SignalStrength (float) = -50.0f (0x00 00 48 C2)
            // Latitude (double) = 12.34 (0x5C 8F C2 F5 28 5C 28 40)
            // Longitude (double) = 56.78 (0x52 B8 1E 85 EB 63 4C 40)
            // StatusCodes (uint) = 1 (0x01 00 00 00)
            
            byte[] rawData = new byte[] {
                0x4B, 0x00, 0x00, 0x00, // Battery
                0x00, 0x00, 0x48, 0xC2, // Signal
                0x5C, 0x8F, 0xC2, 0xF5, 0x28, 0x5C, 0x28, 0x40, // Lat
                0x52, 0xB8, 0x1E, 0x85, 0xEB, 0x63, 0x4C, 0x40, // Lon
                0x01, 0x00, 0x00, 0x00  // Status
            };

            // Act
            TelemetryFrame frame = ProtocolMapper.FromBytes(rawData);

            // Assert
            Assert.Equal(75, frame.BatteryLevel);
            Assert.Equal(-50.0f, frame.SignalStrength);
            Assert.Equal(12.34, frame.Latitude, 3);
            Assert.Equal(56.78, frame.Longitude, 3);
            Assert.Equal(1u, frame.StatusCodes);
        }

        [Fact]
        public void StructSize_ShouldBeExactly28Bytes()
        {
            // Assert
            Assert.Equal(28, Marshal.SizeOf<TelemetryFrame>());
        }
    }
}
