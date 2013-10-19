using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace AR_Drone_Remote_for_Windows_8
{
    [TestClass]
    public class StringConverterTests
    {
        [TestMethod]
        public void Convert_GivenFloatAndStringFormat_ReturnsFormattedString()
        {
            // Arrange
            var target = new StringConverter();
            const float value = 39.812345f;
            const string format = "{0:0.0}";

            // Act
            var result = (string)target.Convert(value, null, format, string.Empty);

            // Assert
            result.Should().Be("39.8");
        }
    }
}
