using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class VideoWorkerTests
    {
        private VideoWorker _target;
        private Mock<ITcpSocket> _mockTcpSocket;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockTcpSocket = new Mock<ITcpSocket>();
            _target = new VideoWorker {Socket = _mockTcpSocket.Object};
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
