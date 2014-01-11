using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class DoubleToInt64ConverterTests
    {
        private DoubleToInt64Converter _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new DoubleToInt64Converter();
        }

        [TestMethod]
        public void GivenDouble_Convert_ReturnsBitIdenticalInt64()
        {
            VerifyDoubleConvertsToInt64(987.654, 4651898712276737196);
            VerifyDoubleConvertsToInt64(42.0, 4631107791820423168);
        }

        private void VerifyDoubleConvertsToInt64(double testValue, long expectedResult)
        {
            // Act
            var result = _target.Convert(testValue);

            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
