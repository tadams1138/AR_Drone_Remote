using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class DateTimeFactoryTests
    {
        private DateTimeFactory _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new DateTimeFactory();
        }

        [TestMethod]
        public void Now_ReturnsCurrentTime()
        {
            // Arrange
            DateTime expectedResult = DateTime.Now;

            // Act
            var result = _target.Now;

            // Assert
            result.Should().BeCloseTo(expectedResult);
        }
    }
}
