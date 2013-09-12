using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Windows.System;
using Windows.UI.Core;

namespace AR_Drone_Remote_for_Windows_8
{
    [TestClass]
    public class KeyStateIndicatorTests
    {
        private const VirtualKey Key = VirtualKey.A;
        private KeyStateIndicator _target;
        private Mock<ICoreWindow> _mockCoreWindow;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockCoreWindow = new Mock<ICoreWindow>();
            _target = new KeyStateIndicator { CoreWindow = _mockCoreWindow.Object };
        }

        [TestMethod]
        public void IsKeyDown_KeyIsDown_ReturnTrue()
        {
            // Arrange
            _mockCoreWindow.Setup(x => x.GetAsyncKeyState(Key)).Returns(CoreVirtualKeyStates.Down);

            // Act
            bool result = _target.IsKeyDown(Key);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsKeyDown_KeyIsLocked_ReturnTrue()
        {
            // Arrange
            _mockCoreWindow.Setup(x => x.GetAsyncKeyState(Key)).Returns(CoreVirtualKeyStates.Locked);

            // Act
            bool result = _target.IsKeyDown(Key);

            // Assert
            result.Should().BeTrue();
        }

        [TestMethod]
        public void IsKeyDown_KeyIsUp_ReturnFalse()
        {
            // Arrange
            _mockCoreWindow.Setup(x => x.GetAsyncKeyState(Key)).Returns(CoreVirtualKeyStates.None);

            // Act
            bool result = _target.IsKeyDown(Key);

            // Assert
            result.Should().BeFalse();
        }
    }
}
