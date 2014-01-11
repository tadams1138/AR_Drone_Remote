using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class FloatToInt32ConverterTests
    {
        private FloatToInt32Converter _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new FloatToInt32Converter();
        }

        [TestMethod]
        public void GivenFloat_Convert_ReturnsIntWithIdenticalBinary()
        {
            VerifyFloatConvertsToInt32(0.0f, 0);
            VerifyFloatConvertsToInt32(1.1f, 1066192077);
            VerifyFloatConvertsToInt32(-987.654f, -998839845);
        }

        private void VerifyFloatConvertsToInt32(float floatTestValue, int expectedInt)
        {
            var result = _target.Convert(floatTestValue);
            result.Should().Be(expectedInt);
        }
    }
}
