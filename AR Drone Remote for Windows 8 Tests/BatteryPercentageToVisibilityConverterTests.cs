using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.UI.Xaml;

namespace AR_Drone_Remote_for_Windows_8
{
    [TestClass]
    public class BatteryPercentageToVisibilityConverterTests
    {
        private BatteryPercentageToVisibilityConverter _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new BatteryPercentageToVisibilityConverter();
        }

        [TestMethod]
        public void Convert_ValueAboveThreshold_ReturnVisible()
        {
            // Arrange
            const uint value = 7;
            const string threshold = "5";

            // Act
            var result = _target.Convert(value, null, threshold, string.Empty);

            // Assert
            result.Should().Be(Visibility.Visible);
        }

        [TestMethod]
        public void Convert_ValueBelowThreshold_ReturnNotVisible()
        {
            // Arrange
            const uint value = 7;
            const string threshold = "15";

            // Act
            var result = _target.Convert(value, null, threshold, string.Empty);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }

        [TestMethod]
        public void Convert_ValueAtThreshold_ReturnNotVisible()
        {
            // Arrange
            const uint value = 7;
            const string threshold = "7";

            // Act
            var result = _target.Convert(value, null, threshold, string.Empty);

            // Assert
            result.Should().Be(Visibility.Collapsed);
        }
    }
}