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

        [TestMethod]
        public void OnSocketReceivedEvent_ReturnsOutOfSequenceData_DoesNotRaiseEvent()
        {
            // Arrange
            NavData.NavData navDataReceived = null;
            _target.NavDataReceived += (sender, args) => navDataReceived = args.NavData;

            var firstData = new NavData.NavData { Sequence = 6 };
            var firstBytes = new byte[] { 1 };
            var firstDataReceivedEventArgs = new DataReceivedEventArgs(firstBytes);
            _mockNavDataFactory.Setup(x => x.Create(firstBytes)).Returns(firstData);

            var secondData = new NavData.NavData { Sequence = 5 };
            var secondBytes = new byte[] { 2 };
            var secondDataReceivedEventArgs = new DataReceivedEventArgs(secondBytes);
            _mockNavDataFactory.Setup(x => x.Create(secondBytes)).Returns(secondData);

            _target.Run();

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, firstDataReceivedEventArgs);

            // Assert
            navDataReceived.Should().Be(firstData);

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, secondDataReceivedEventArgs);

            // Assert
            navDataReceived.Should().Be(firstData);
        }

        [TestMethod]
        public void OnSocketReceivedEvent_ReturnsInSequenceData_RaisesEvents()
        {
            // Arrange
            NavData.NavData navDataReceived = null;
            _target.NavDataReceived += (sender, args) => navDataReceived = args.NavData;

            var firstData = new NavData.NavData { Sequence = 1 };
            var firstBytes = new byte[] { 1 };
            var firstDataReceivedEventArgs = new DataReceivedEventArgs(firstBytes);
            _mockNavDataFactory.Setup(x => x.Create(firstBytes)).Returns(firstData);

            var secondData = new NavData.NavData { Sequence = 2 };
            var secondBytes = new byte[] { 2 };
            var secondDataReceivedEventArgs = new DataReceivedEventArgs(secondBytes);
            _mockNavDataFactory.Setup(x => x.Create(secondBytes)).Returns(secondData);

            _target.Run();

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, firstDataReceivedEventArgs);

            // Assert
            navDataReceived.Should().Be(firstData);

            // Act
            _mockUdpSocket.Raise(x => x.DataReceived += null, secondDataReceivedEventArgs);

            // Assert
            navDataReceived.Should().Be(secondData);
        }

        private void VerifyPacketSent()
        {
            _mockUdpSocket.Verify(x => x.Write(1));
        }
    }
}