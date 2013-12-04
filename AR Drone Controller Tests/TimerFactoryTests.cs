using System.Threading;
using AR_Drone_Controller.NavData;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class TimerFactoryTests
    {
        private TimerFactory _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new TimerFactory
            {
                TimerCallback = TimerCallback
            };
        }

        [TestMethod]
        public void CreateCommandTimer_ReturnsTimer()
        {
            // Arrange

            // Act
            var result = _target.CreateTimer();

            // Assert
            result.Should().BeOfType<Timer>();
            result.Dispose();
        }

        private void TimerCallback(object status)
        {
        }
    }
}
