using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class ControlWorkerTests
    {
        private ControlWorker _target;
        private Mock<ITcpSocket> _mockTcpSocket;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockTcpSocket = new Mock<ITcpSocket>();
            _target = new ControlWorker {Socket = _mockTcpSocket.Object};
        }

        [TestMethod]
        public void Run_CallsSocketConnect()
        {
            // Arrange

            // Act
            _target.Run();

            // Assert
            _mockTcpSocket.Verify(x => x.Connect());
        }

        [TestMethod]
        public void SocketDisconnects_RaiseDisconnectEvent()
        {
            // Arrange
            bool disconnectedEventRaised = false;
            _target.Disconnected += (sender, args) => disconnectedEventRaised = true;
            _target.Run();

            // Act
            _mockTcpSocket.Raise(s => s.Disconnected += null, EventArgs.Empty);

            // Assert
            disconnectedEventRaised.Should().BeTrue();
        }

        [TestMethod]
        public void Dispose_DisposesSocket()
        {
            // Arrange

            // Act 
            _target.Dispose();

            // Assert
            _mockTcpSocket.Verify(x  => x.Dispose());
        }
    }
}
