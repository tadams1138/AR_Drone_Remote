using System;
using AR_Drone_Controller.NavData;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class NavDataWorkerTests
    {
        private NavDataWorker _target;
        private Mock<IUdpSocket> _mockUdpSocket;
        private Mock<NavDataFactory> _mockNavDataFactory;
        private Mock<TimerFactory> _mockTimerFactory;
        private Mock<IDisposable> _mockTimer;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockUdpSocket = new Mock<IUdpSocket>();
            _mockNavDataFactory = new Mock<NavDataFactory>();
            _mockTimerFactory = new Mock<TimerFactory>();
            _mockTimer = new Mock<IDisposable>();
            _target = new NavDataWorker { Socket = _mockUdpSocket.Object, NavDataFactory = _mockNavDataFactory.Object, TimerFactory = _mockTimerFactory.Object };
        }

        [TestMethod]
        public void Run_CallsSocketConnectStartsTimerAndInitiatesCommunication()
        {
            // Arrange

            // Act
            _target.Run();

            // Assert
            _mockUdpSocket.Verify(x => x.Connect());
            _mockTimerFactory.Verify(x => x.CreateTimer());
            VerifyPacketSent();
        }

        [TestMethod]
        public void Dispose_DisposesSocketAndTimer()
        {
            // Arrange
            _mockTimerFactory.Setup(x => x.CreateTimer()).Returns(_mockTimer.Object);
            _target.Run();

            // Act 
            _target.Dispose();

            // Assert
            _mockUdpSocket.Verify(x => x.Dispose());
            _mockTimer.Verify(x => x.Dispose());
        }

        [TestMethod]
        public void GivenIncompleteInitialization_Dispose_CannotThrowException()
        {
            // Arrange

            // Act
            _target.Dispose();

            // Assert
            // no exception
        }

        [TestMethod]
        public void OnSocketReceiveEvent_NavDataReceivedEventRaised()
        {
            // Arrange
            var expectedResult = new NavData.NavData();
            var bytes = new byte[] { 0 };
            var dataReceivedEventArgs = new DataReceivedEventArgs(bytes);
            NavData.NavData navDataReceived = null;
            _target.NavDataReceived += (sender, args) => navDataReceived = args.NavData;
            _mockNavDataFactory.Setup(x => x.Create(bytes)).Returns(expectedResult);
            _target.Run();

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, dataReceivedEventArgs);

            // Assert
            navDataReceived.Should().Be(expectedResult);
        }

        [TestMethod]
        public void OnSocketReceivedEvent_ReturnsNoData_DoesNotRaiseEvent()
        {
            // Arrange
            bool navDataReceivedEventRaised = false;
            _target.NavDataReceived += (sender, args) => navDataReceivedEventRaised = true;
            _target.Run();

            // Act
            var dataReceivedEventArgs = new DataReceivedEventArgs(null);
            _mockUdpSocket.Raise(x => x.DataReceived += null, dataReceivedEventArgs);

            // Assert
            navDataReceivedEventRaised.Should().BeFalse();
        }

        [TestMethod]
        public void NavDataFactoryThrowsException_ExceptionIsIgnored()
        {
            // Arrange
            var bytes = new byte[] { 0 };
            var dataReceivedEventArgs = new DataReceivedEventArgs(bytes);
            bool navDataReceivedEventRaised = false;
            _target.NavDataReceived += (sender, args) => navDataReceivedEventRaised = true;
            _mockNavDataFactory.Setup(x => x.Create(bytes)).Throws(new Exception("This exception should have been ignored."));
            _target.Run();

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, dataReceivedEventArgs);

            // Assert
            navDataReceivedEventRaised.Should().BeFalse();
        }

        [TestMethod]
        public void GivenNoDataReceived_WhenTimeoutExceeded_CommunicationReInitiated()
        {
            // Arrange
            _target.Run();
            
            // Act
            _target.CheckTimeout(null);

            // Assert
            _mockUdpSocket.Verify(x => x.Write(1), Times.Exactly(2));
        }

        [TestMethod]
        public void GivenDataReceived_WhenTimeoutExceeded_NoPacketSent()
        {
            // Arrange
            _target.Run();
            _mockUdpSocket.Raise(x => x.DataReceived += null, new DataReceivedEventArgs(null));

            // Act
            _target.CheckTimeout(null);

            // Assert
            _mockUdpSocket.Verify(x => x.Write(1), Times.Exactly(1));
        }

        private void VerifyPacketSent()
        {
            _mockUdpSocket.Verify(x => x.Write(1));
        }
    }
}