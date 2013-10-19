using System;
using AR_Drone_Controller.NavData;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class DroneControllerTests
    {
        private DroneController _target;
        private Mock<WorkerFactory> _mockWorkerFactory;
        private Mock<ISocketFactory> _mockSocketFactory;
        private ConnectParams _connectArgs;
        private Mock<ControlWorker> _mockControlWorker;
        private Mock<CommandWorker> _mockCmdWorker;
        private Mock<VideoWorker> _mockVideoWorker;
        private Mock<NavDataWorker> _mockNavDataWorker;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new DroneController();
        }

        [TestMethod]
        public void Constructor_SetsPropertiesToDefaults()
        {
            // Arrange

            // Act

            // Assert
            AssertPropertiesAreSetToDefaults();
        }

        [TestMethod]
        public void Disconnect_ClosesAllOpenWorkersResetsPropertiesToDefaults()
        {
            // Arrange
            SetPropertiesToNonDefaultValues();
            InitializeCtrlCmdVideoAndNavDataWorkerMocks();

            // Act
            _target.Disconnect();

            // Assert
            VerifyAllWorkersDisposed();
            AssertPropertiesAreSetToDefaults();
        }

        [TestMethod]
        public void Constructor_InstantiatesWorkerFactory()
        {
            // Arrange

            // Act

            // Assert
            _target.WorkerFactory.Should().NotBeNull();
        }

        [TestMethod]
        public void Connect_WhenNotConnected_InitializesNewWorkersAndSetsConnectTrue()
        {
            // Arrange
            InitializeFactoriesAndConnectParams();

            // Act
            _target.Connect(_connectArgs);

            // Assert
            VerifyNewWorkersCreated();
            _target.Connected.Should().BeTrue();
        }

        [TestMethod]
        public void Connect_WhenConnected_ClosesOldWorkersAndInitializesNewWorkersAndSetsConnectTrue()
        {
            // Arrange
            InitializeFactoriesAndConnectParams();
            InitializeCtrlCmdVideoAndNavDataWorkerMocks();

            // Act
            _target.Connect(_connectArgs);

            // Assert
            VerifyAllWorkersDisposed();
            VerifyNewWorkersCreated();
            _target.Connected.Should().BeTrue();
        }

        [TestMethod]
        public void Connect_ExceptionThrown_ClosesAnyWorkersAndSetsPropertiesToDefaults()
        {
            // Arrange
            InitializeFactoriesAndConnectParams();
            InitializeCtrlCmdAndVideoWorkerMocks();

            _mockWorkerFactory.Setup(x => x.CreateControlWorker(_mockSocketFactory.Object, _connectArgs))
                             .Returns(_mockControlWorker.Object);
            _mockWorkerFactory.Setup(x => x.CreateCommandWorker(_mockSocketFactory.Object, _connectArgs))
                             .Returns(_mockCmdWorker.Object);
            _mockWorkerFactory.Setup(x => x.CreateNavDataWorker(_mockSocketFactory.Object, _connectArgs))
                             .Throws(new Exception("Testing"));

            Exception expectedException = null;

            // Act
            try
            {
                _target.Connect(_connectArgs);
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            VerifyCtrlCmdAndVideoWorkersDisposed();
            AssertPropertiesAreSetToDefaults();
            expectedException.Should().NotBeNull();
        }

        private void VerifyAllWorkersDisposed()
        {
            VerifyCtrlCmdAndVideoWorkersDisposed();
            _mockNavDataWorker.Verify(x => x.Dispose());
        }

        private void InitializeCtrlCmdVideoAndNavDataWorkerMocks()
        {
            InitializeCtrlCmdAndVideoWorkerMocks();
            _mockNavDataWorker = new Mock<NavDataWorker>();
            _target._controlWorker = _mockControlWorker.Object;
            _target._commandWorker = _mockCmdWorker.Object;
            _target._navDataWorker = _mockNavDataWorker.Object;
        }

        private void VerifyCtrlCmdAndVideoWorkersDisposed()
        {
            _mockControlWorker.Verify(x => x.Dispose());
            _mockCmdWorker.Verify(x => x.Dispose());
            _mockVideoWorker.Verify(x => x.Dispose());
        }

        private void InitializeCtrlCmdAndVideoWorkerMocks()
        {
            _mockControlWorker = new Mock<ControlWorker>();
            _mockCmdWorker = new Mock<CommandWorker>();
            _mockVideoWorker = new Mock<VideoWorker>();
            _target._videoWorker = _mockVideoWorker.Object;
        }

        private void VerifyNewWorkersCreated()
        {
            _mockWorkerFactory.Verify(x => x.CreateControlWorker(_mockSocketFactory.Object, _connectArgs));
            _mockWorkerFactory.Verify(x => x.CreateCommandWorker(_mockSocketFactory.Object, _connectArgs));
            _mockWorkerFactory.Verify(x => x.CreateNavDataWorker(_mockSocketFactory.Object, _connectArgs));
        }

        private void InitializeFactoriesAndConnectParams()
        {
            _mockWorkerFactory = new Mock<WorkerFactory>();
            _mockSocketFactory = new Mock<ISocketFactory>();
            _connectArgs = new ConnectParams();
            _target.SocketFactory = _mockSocketFactory.Object;
            _target.WorkerFactory = _mockWorkerFactory.Object;
        }

        private void SetPropertiesToNonDefaultValues()
        {
            _target.Altitude = 13;
            _target.BatteryPercentage = 13;
            _target.Theta = 13;
            _target.Psi = 13;
            _target.Phi = 13;
            _target.KilometersPerHour = 13;
            _target.Connected = true;
            _target.CanSendFlatTrimCommand = true;
            _target.Flying = true;
            _target.CanRecord = true;
            _target.UsbKeyIsRecording = true;
        }

        private void AssertPropertiesAreSetToDefaults()
        {
            _target.Altitude.Should().Be(0);
            _target.BatteryPercentage.Should().Be(0);
            _target.Theta.Should().Be(0);
            _target.Psi.Should().Be(0);
            _target.Phi.Should().Be(0);
            _target.KilometersPerHour.Should().Be(0);
            _target.Connected.Should().BeFalse();
            _target.CanSendFlatTrimCommand.Should().BeFalse();
            _target.Flying.Should().BeFalse();
            _target.CanRecord.Should().BeFalse();
            _target.UsbKeyIsRecording.Should().BeFalse();
        }
    }
}
