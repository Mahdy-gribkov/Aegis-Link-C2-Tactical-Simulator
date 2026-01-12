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
            short az = 12345;
            short el = 6789;
            byte flags = 0x01;
            byte battery = 90;

            // Build byte array (6 bytes)
            byte[] rawData = new byte[6];
            Buffer.BlockCopy(BitConverter.GetBytes(az), 0, rawData, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(el), 0, rawData, 2, 2);
            rawData[4] = flags;
            rawData[5] = battery;

            // Act
            TelemetryFrame frame = ProtocolMapper.FromBytes(rawData);

            // Assert
            Assert.Equal(az, frame.AzimuthScaled);
            Assert.Equal(el, frame.ElevationScaled);
            Assert.Equal(flags, frame.StatusFlags);
            Assert.Equal(battery, frame.BatteryLevel);
            Assert.Equal(123.45f, frame.Azimuth, 2);
        }

        [Fact]
        public void StructSize_ShouldBeExactly6Bytes()
        {
            // Assert
            Assert.Equal(6, Marshal.SizeOf<TelemetryFrame>());
        }
    }
}
