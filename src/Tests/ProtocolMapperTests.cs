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
            byte version = TacticalConstants.PROTOCOL_VERSION;
            short az = 12345;
            short el = 6789;
            byte flags = 0x01;
            byte battery = 90;

            // Build byte array (7 bytes)
            byte[] rawData = new byte[7];
            rawData[0] = version;
            Buffer.BlockCopy(BitConverter.GetBytes(az), 0, rawData, 1, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(el), 0, rawData, 3, 2);
            rawData[5] = flags;
            rawData[6] = battery;

            // Act
            TelemetryFrame frame = ProtocolMapper.FromBytes(rawData);

            // Assert
            Assert.Equal(version, frame.Version);
            Assert.Equal(az, frame.AzimuthScaled);
            Assert.Equal(el, frame.ElevationScaled);
            Assert.Equal(flags, frame.StatusFlags);
            Assert.Equal(battery, frame.BatteryLevel);
            Assert.Equal(123.45f, frame.Azimuth, 2);
        }

        [Fact]
        public void StructSize_ShouldBeExactly7Bytes()
        {
            // Assert
            Assert.Equal(7, Marshal.SizeOf<TelemetryFrame>());
        }
    }
}
