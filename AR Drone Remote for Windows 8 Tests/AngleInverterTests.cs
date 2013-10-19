using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace AR_Drone_Remote_for_Windows_8
{
    [TestClass]
    public class AngleInverterTests
    {
        private AngleInverter _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new AngleInverter();
        }

        [TestMethod]
        public void Convert_InvertsRotationDirection()
        {
            // Arrange 
            var invertedPairs = new Dictionary<double, double>
                {
                    {0, 0},
                    {1, 359},
                    {45, 315},
                    {90, 270},
                    {180, 180},
                    {270, 90},
                    {315, 45},
                    {359, 1}
                };

            foreach (var pair in invertedPairs)
            {
                // Act
                object result = _target.Convert(pair.Key, null, null, string.Empty);

                // Assert
                result.Should().Be(pair.Value);
            }
        }


        [TestMethod]
        public void Convert_GivenFloat_ReturnsDouble()
        {
            // Arrange 
            const float value = 90;

            // Act
            object result = _target.Convert(value, null, null, string.Empty);

            // Assert
            result.Should().BeOfType<double>();
        }

        [TestMethod]
        public void Convert_GivenDouble_ReturnsDouble()
        {
            // Arrange 
            const double value = 90;

            // Act
            object result = _target.Convert(value, null, null, string.Empty);

            // Assert
            result.Should().BeOfType<double>();
        }

        [TestMethod]
        public void Convert_GivenNegative_ReturnsValueBetween0and360()
        {
            // Arrange 
            const double value = -90;

            // Act
            object result = _target.Convert(value, null, null, string.Empty);

            // Assert
            result.Should().Be(90.0);
        }

        [TestMethod]
        public void Convert_GivenValueGreaterThan360_ReturnsValueBetween0and360()
        {
            // Arrange 
            const double value = 450;

            // Act
            object result = _target.Convert(value, null, null, string.Empty);

            // Assert
            result.Should().Be(270.0);
        }
    }
}
