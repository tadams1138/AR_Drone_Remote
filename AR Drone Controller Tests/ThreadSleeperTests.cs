using System;
using System.Threading;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class ThreadSleeperTests
    {
        private ThreadSleeper _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new ThreadSleeper();
        }

        [TestMethod]
        public void Sleep_Waits()
        {
            // Arrange
            var start = DateTime.UtcNow;
            const int millisecondsToSleep = 30;

            // Act
            _target.Sleep(millisecondsToSleep);
            var end = DateTime.UtcNow;

            // Assert
            double timeToComplete = (end - start).TotalMilliseconds;
            timeToComplete.Should().BeGreaterOrEqualTo(millisecondsToSleep);
        }
    }
}
