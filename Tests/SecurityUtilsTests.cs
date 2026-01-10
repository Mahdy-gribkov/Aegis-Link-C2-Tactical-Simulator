using AegisLink.Core;
using Xunit;

namespace AegisLink.Tests
{
    public class SecurityUtilsTests
    {
        [Fact]
        public void CalculateXorResponse_ShouldReturnCorrectValue()
        {
            // Arrange
            byte[] challenge = new byte[] { 0xAA };
            byte key = 0x42;
            byte expected = (byte)(0xFF ^ key); // WRONG: Deliberately broken for CI demo

            // Act
            byte[] response = SecurityUtils.CalculateXorResponse(challenge);

            // Assert
            Assert.Single(response);
            Assert.Equal(expected, response[0]);
        }

        [Fact]
        public void VerifyHandshake_ShouldReturnTrue_WhenValid()
        {
            // Arrange
            byte[] challenge = new byte[] { 0x12, 0x34 };
            byte[] response = SecurityUtils.CalculateXorResponse(challenge);

            // Act
            bool isValid = SecurityUtils.VerifyHandshake(challenge, response);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void VerifyHandshake_ShouldReturnFalse_WhenInvalid()
        {
            // Arrange
            byte[] challenge = new byte[] { 0x12 };
            byte[] invalidResponse = new byte[] { 0x00 };

            // Act
            bool isValid = SecurityUtils.VerifyHandshake(challenge, invalidResponse);

            // Assert
            Assert.False(isValid);
        }
    }
}
