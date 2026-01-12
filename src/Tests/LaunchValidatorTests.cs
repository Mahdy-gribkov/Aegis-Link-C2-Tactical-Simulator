using AegisLink.Core;
using Xunit;

namespace AegisLink.Tests
{
    public class LaunchValidatorTests
    {
        [Fact]
        public void IsSafeToLaunch_ShouldReturnTrue_OnlyWhenAllChecksPass()
        {
            var frameReady = new TelemetryFrame(0, 0, true, true, 20);
            var frameNoBatt = new TelemetryFrame(0, 0, true, true, 5);
            var frameNoArm = new TelemetryFrame(0, 0, false, true, 20);
            var frameNoLock = new TelemetryFrame(0, 0, true, false, 20);

            Assert.True(LaunchValidator.IsSafeToLaunch(frameReady));
            Assert.False(LaunchValidator.IsSafeToLaunch(frameNoBatt));
            Assert.False(LaunchValidator.IsSafeToLaunch(frameNoArm));
            Assert.False(LaunchValidator.IsSafeToLaunch(frameNoLock));
        }

        [Fact]
        public void GetSafetyReport_ShouldReturnCritical_WhenMultipleFailures()
        {
            // Arrange
            var frame = new TelemetryFrame(0, 0, false, false, 5); 

            // Act
            var report = LaunchValidator.GetSafetyReport(frame);

            // Assert
            Assert.Contains("LOW BATT", report);
            Assert.Contains("UNARMED", report);
            Assert.Contains("NO LOCK", report);
        }

        [Fact]
        public void GetSafetyReport_ShouldReturnReady_WhenAllChecksPass()
        {
            // Arrange
            var frame = new TelemetryFrame(10, 20, true, true, 80);

            // Act
            var report = LaunchValidator.GetSafetyReport(frame);

            // Assert
            Assert.Equal("READY FOR LAUNCH", report);
        }
    }
}
