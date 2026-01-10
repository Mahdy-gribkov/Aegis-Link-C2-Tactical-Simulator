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
            int battery = 75;
            float signal = -50.0f;
            double lat = 12.34;
            double lon = 56.78;
            uint status = 1u;

            // Build byte array dynamically to match system endianness
            byte[] rawData = new byte[28];
            Buffer.BlockCopy(BitConverter.GetBytes(battery), 0, rawData, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(signal), 0, rawData, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(lat), 0, rawData, 8, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(lon), 0, rawData, 16, 8);
            Buffer.BlockCopy(BitConverter.GetBytes(status), 0, rawData, 24, 4);

            // Act
            TelemetryFrame frame = ProtocolMapper.FromBytes(rawData);

            // Assert
            Assert.Equal(battery, frame.BatteryLevel);
            Assert.Equal(signal, frame.SignalStrength, 2); // Use 2 decimal places precision
            Assert.Equal(lat, frame.Latitude, 2);
            Assert.Equal(lon, frame.Longitude, 2);
            Assert.Equal(status, frame.StatusCodes);
        }

        [Fact]
        public void StructSize_ShouldBeExactly28Bytes()
        {
            // Assert
            Assert.Equal(28, Marshal.SizeOf<TelemetryFrame>());
        }
    }
}
