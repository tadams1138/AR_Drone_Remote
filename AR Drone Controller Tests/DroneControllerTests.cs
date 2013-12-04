using System;
using System.Collections.Generic;
using System.Threading;
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
        private Mock<CommandWorker> _mockCommandWorker;
        private Mock<VideoWorker> _mockVideoWorker;
        private Mock<NavDataWorker> _mockNavDataWorker;
        private Mock<DemoOption> _demoArgs;
        private Mock<NavData.NavData> _navDataArgs;
        private Mock<HdVideoStreamOption> _hdVideoStreamArgs;
        private Mock<IDisposable> _mockCommandTimer;
        private Mock<IDispatcher> _mockDispatcher;
        private Mock<TimerFactory> _mockTimerFactory;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new DroneController();
        }

        [TestMethod]
        public void HelpUri_ReturnsUri()
        {
            _target.HelpUri.Should().BeOfType<Uri>();
        }

        [TestMethod]
        public void PrivacyUri_ReturnsUri()
        {
            _target.PrivacyUri.Should().BeOfType<Uri>();
        }

        [TestMethod]
        public void LedAnimations_ReturnsListOfLedAnimations()
        {
            _target.LedAnimations.Should().BeOfType<List<LedAnimation>>();
        }

        [TestMethod]
        public void FlightAnimations_ReturnsListOfFlightAnimations()
        {
            _target.FlightAnimations.Should().BeOfType<List<FlightAnimation>>();
        }

        #region Constructor tests

        [TestMethod]
        public void Constructor_SetsPropertiesToDefaults()
        {
            // Arrange

            // Act

            // Assert
            AssertPropertiesAreSetToDefaults();
        }

        [TestMethod]
        public void Constructor_InstantiatesFactories()
        {
            // Arrange

            // Act

            // Assert
            _target.WorkerFactory.Should().BeOfType<WorkerFactory>();
            _target.TimerFactory.Should().BeOfType<TimerFactory>();
            _target.TimerFactory.TimerCallback.Should().Be((TimerCallback)_target.DoWork);
            _target.TimerFactory.Period.Should().Be(DroneController.OptimalDelayBetweenCommandsInMilliseconds);
        }

        #endregion

        [TestMethod]
        public void SocketFactory_AssignedToWorkerFactory()
        {
            // Arrange
            _mockSocketFactory = new Mock<ISocketFactory>();

            // Act
            _target.SocketFactory = _mockSocketFactory.Object;

            // Assert
            _target.WorkerFactory.SocketFactory.Should().Be(_mockSocketFactory.Object);
        }

        [TestMethod]
        public void ConnectParams_AssignedToWorkerFactory()
        {
            // Arrange
            _connectArgs = new ConnectParams();

            // Act
            _target.ConnectParams = _connectArgs;

            // Assert
            _target.WorkerFactory.ConnectParams.Should().Be(_connectArgs);
        }

        [TestMethod]
        public void Disconnect_ClosesAllOpenWorkersAndTimersAndResetsPropertiesToDefaults()
        {
            // Arrange
            SetPropertiesToNonDefaultValues();

            _mockControlWorker = new Mock<ControlWorker>();
            _mockCommandWorker = new Mock<CommandWorker>();
            _mockVideoWorker = new Mock<VideoWorker>();
            _target.VideoWorker = _mockVideoWorker.Object;
            _mockNavDataWorker = new Mock<NavDataWorker>();
            _target.ControlWorker = _mockControlWorker.Object;
            _target.CommandWorker = _mockCommandWorker.Object;
            _target.NavDataWorker = _mockNavDataWorker.Object;
            _mockCommandTimer = new Mock<IDisposable>();
            _target.CommandTimer = _mockCommandTimer.Object;

            // Act
            _target.Disconnect();

            // Assert
            _mockControlWorker.Verify(x => x.Dispose());
            _mockCommandWorker.Verify(x => x.Dispose());
            _mockVideoWorker.Verify(x => x.Dispose());
            _mockNavDataWorker.Verify(x => x.Dispose());
            _mockCommandTimer.Verify(x => x.Dispose());
            AssertPropertiesAreSetToDefaults();
        }

        #region Connect tests

        [TestMethod]
        public void Connect_WhenNotConnected_InitializesNewWorkersAndSetsConnectTrue()
        {
            // Arrange
            InitializeFactoryAndWorkerMocks();

            // Act
            _target.Connect();

            // Assert
            VerifyNewWorkersAndTimerRunAndConnectedSetToTrue();
        }

        [TestMethod]
        public void Connect_WhenConnected_ClosesOldWorkersAndTimerRunsNewWorkersAndSetsConnectTrue()
        {
            // Arrange
            InitializeFactoryAndWorkerMocks();

            var mockControlWorker = new Mock<ControlWorker>();
            var mockCmdWorker = new Mock<CommandWorker>();
            var mockNavDataWorker = new Mock<NavDataWorker>();
            var mockVideoWorker = new Mock<VideoWorker>();
            var mockCommandTimer = new Mock<IDisposable>();
            _target.VideoWorker = mockVideoWorker.Object;
            _target.ControlWorker = mockControlWorker.Object;
            _target.CommandWorker = mockCmdWorker.Object;
            _target.NavDataWorker = mockNavDataWorker.Object;
            _target.CommandTimer = mockCommandTimer.Object;

            // Act
            _target.Connect();

            // Assert
            mockControlWorker.Verify(x => x.Dispose());
            mockCmdWorker.Verify(x => x.Dispose());
            mockVideoWorker.Verify(x => x.Dispose());
            mockNavDataWorker.Verify(x => x.Dispose());
            mockCommandTimer.Verify(x => x.Dispose());
            VerifyNewWorkersAndTimerRunAndConnectedSetToTrue();
        }

        [TestMethod]
        public void Connect_ExceptionThrown_ClosesAnyWorkersAndTimersAndSetsPropertiesToDefaults()
        {
            // Arrange
            Exception expectedException = null;
            InitializeFactoryAndWorkerMocks();
            _target.VideoWorker = _mockVideoWorker.Object;
            _mockNavDataWorker.Setup(x => x.Run()).Throws(new Exception("Test Exception"));

            // Act
            try
            {
                _target.Connect();
            }
            catch (Exception ex)
            {
                expectedException = ex;
            }

            // Assert
            VerifyAllWorkersAndTimersDisposed();
            AssertPropertiesAreSetToDefaults();
            expectedException.Should().NotBeNull();
        }

        [TestMethod]
        public void ControlWorkerDisconnects_ClosesAnyWorkersAndTimersAndSetsPropertiesToDefaults()
        {
            // Arrange
            InitializeFactoryAndWorkerMocks();
            _target.VideoWorker = _mockVideoWorker.Object;
            _target.Connect();

            // Act
            _mockControlWorker.Raise(x => x.Disconnected += null, EventArgs.Empty);

            // Assert
            VerifyAllWorkersAndTimersDisposed();
            AssertPropertiesAreSetToDefaults();
        }

        #endregion

        #region Property update tests

        [TestMethod]
        public void NavDataWorkerReceives_DispatcherInvokes()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _mockDispatcher.Verify(x => x.BeginInvoke(It.IsAny<Action>()));
        }

        [TestMethod]
        public void WhenDisconnected_NavDataWorkerReceives_NothingIsAssigned()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _target.Connected = false;

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _mockDispatcher.Verify(x => x.BeginInvoke(It.IsAny<Action>()), Times.Never());
        }

        [TestMethod]
        public void NavDataWorkerReceivesAltitude_SetsAltitudeProperty()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const int altitude = 100;
            _demoArgs.Setup(x => x.Altitude).Returns(altitude);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.Altitude.Should().Be(altitude);
        }

        [TestMethod]
        public void NavDataWorkerReceivesBatteryPercentage_SetsBatteryPercentageProperty()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const int battery = 100;
            _demoArgs.Setup(x => x.BatteryPercentage).Returns(battery);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.BatteryPercentage.Should().Be(battery);
        }

        [TestMethod]
        public void NavDataWorkerReceivesCanRecord_SetsCanRecordProperty()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _hdVideoStreamArgs.Setup(x => x.CanRecord).Returns(true);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.CanRecord.Should().BeTrue();
        }

        [TestMethod]
        public void NavDataWorkerReceivesNotFlying_SetsCanSendFlatTrimProperty()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.Flying).Returns(false);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.CanSendFlatTrimCommand.Should().BeTrue();
        }

        [TestMethod]
        public void NavDataWorkerReceivesFlying_SetsFlyingProperty()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.Flying).Returns(true);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.Flying.Should().BeTrue();
        }

        [TestMethod]
        public void NavDataWorkerReceivesKilometersPerHour_SetsKilometersPerHour()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const float kph = 100;
            _demoArgs.Setup(x => x.KilometersPerHour).Returns(kph);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.KilometersPerHour.Should().Be(kph);
        }

        [TestMethod]
        public void NavDataWorkerReceivesPhi_SetsPhi()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const float phi = 100;
            _demoArgs.Setup(x => x.Phi).Returns(phi);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.Phi.Should().Be(phi);
        }

        [TestMethod]
        public void NavDataWorkerReceivesPsi_SetsPsi()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const float psi = 100;
            _demoArgs.Setup(x => x.Psi).Returns(psi);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.Psi.Should().Be(psi);
        }

        [TestMethod]
        public void NavDataWorkerReceivesTheta_SetsTheta()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            const float theta = 100;
            _demoArgs.Setup(x => x.Theta).Returns(theta);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.Theta.Should().Be(theta);
        }

        [TestMethod]
        public void NavDataWorkerReceivesUsbKeyIsRecording_SetsUsbKeyIsRecording()
        {
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _hdVideoStreamArgs.Setup(x => x.UsbKeyIsRecording).Returns(true);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.UsbKeyIsRecording.Should().BeTrue();
        }

        [TestMethod]
        public void NavDataWorkerReceivesComWatchdog_SetsComWatchdog()
        {
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.CommWatchDog).Returns(true);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.CommWatchDog.Should().BeTrue();
        }

        #endregion

        #region Command Tests

        [TestMethod]
        public void TakeOff_SendsTakeOffCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(x => x.SendRefCommand(CommandWorker.RefCommands.TakeOff));
        }

        [TestMethod]
        public void Land_SendsLandCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.Land();

            // Assert
            _mockCommandWorker.Verify(x => x.SendRefCommand(CommandWorker.RefCommands.LandOrReset));
        }

        [TestMethod]
        public void CalibrateCompass_SendsCalibrateCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.CailbrateCompass();

            // Assert
            _mockCommandWorker.Verify(x => x.SendCalibrateCompassCommand());
        }

        [TestMethod]
        public void FlatTrim_SendsFlatTrimToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.FlatTrim();

            // Assert
            _mockCommandWorker.Verify(x => x.SendFlatTrimCommand());
        }

        [TestMethod]
        public void Emergency_SendsEmergencyCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.Emergency();

            // Assert
            _mockCommandWorker.Verify(x => x.SendRefCommand(CommandWorker.RefCommands.Emergency));
        }

        [TestMethod]
        public void SendLedAnimationCommand_SendsLedAnimationCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            var mockLedAnimation = new Mock<ILedAnimation>();

            // Act
            _target.SendLedAnimationCommand(mockLedAnimation.Object);

            // Assert
            _mockCommandWorker.Verify(x => x.SendLedAnimationCommand(mockLedAnimation.Object));
        }

        [TestMethod]
        public void SendFlightAnimationCommand_SendsFlightAnimationCommandToCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            var mockFlightAnimation = new Mock<IFlightAnimation>();
            // Act
            _target.SendFlightAnimationCommand(mockFlightAnimation.Object);

            // Assert
            _mockCommandWorker.Verify(x => x.SendFlightAnimationCommand(mockFlightAnimation.Object));
        }

        [TestMethod]
        public void WhenNotRecording_StartRecording_StartsVideoWorkerAndSendsStartRecordCommand()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _mockWorkerFactory.Setup(x => x.CreateVideoWorker())
                  .Returns(_mockVideoWorker.Object);

            // Act
            _target.StartRecording();

            // Assert
            VerifyRecordCommandSentAndVideoWorkerStarted();
        }
        
        [TestMethod]
        public void WhenRecording_StartRecording_DisposesExistingVideoWorkerStartsVideoWorkerAndSendsStartRecordCommand()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            var mockVideoWorker = new Mock<VideoWorker>();
            _target.VideoWorker = mockVideoWorker.Object;
            _mockWorkerFactory.Setup(x => x.CreateVideoWorker())
                  .Returns(_mockVideoWorker.Object);

            // Act
            _target.StartRecording();

            // Assert
            VerifyRecordCommandSentAndVideoWorkerStarted();
            mockVideoWorker.Verify(x => x.Dispose());
        }

        [TestMethod]
        public void StopRecording_StopsVideoWorkerAndSendsStopRecordCommandToCmdWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.VideoWorker = _mockVideoWorker.Object;

            // Act
            _target.StopRecording();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoOnUsbConfigKey, DroneController.FalseConfigValue));
            _mockVideoWorker.Verify(x => x.Dispose());
        }

        #endregion

        #region CommandTimer Tick tests

        [TestMethod]
        public void GivenFlying_CommandTimerTick_SendFlightCommands()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.Flying = true;

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify( x => x.SendProgressiveCommand(_target));
            _mockCommandWorker.Verify(x => x.Flush());
        }

        [TestMethod]
        public void GivenCommWatchdogSet_CommandTimerTick_SendResetCommandInstead()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.Flying = true;
            _target.CommWatchDog = true;

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendProgressiveCommand(_target), Times.Never());
            _mockCommandWorker.Verify(x => x.SendResetWatchDogCommand());
            _mockCommandWorker.Verify(x => x.Flush());
        }
        
        #endregion

        private void VerifyRecordCommandSentAndVideoWorkerStarted()
        {
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoOnUsbConfigKey, DroneController.TrueConfigValue));
            _mockCommandWorker.Verify(
                x =>
                x.SendConfigCommand(DroneController.VideoCodecConfigKey, DroneController.H264Codec720PConfigValue));
            _mockVideoWorker.Verify(x => x.Run());
        }

        private void RaiseNavDataReceivedEvent()
        {
            _mockNavDataWorker.Raise(x => x.NavDataReceived += null, new NavDataReceivedEventArgs(_navDataArgs.Object));
        }

        private void InitializeWorkersAndNavDataEventArgsAndConnect()
        {
            InitializeFactoryAndWorkerMocksAndConnect();
            InitializeNavDataEventArgs();
        }

        private void InitializeNavDataEventArgs()
        {
            _demoArgs = new Mock<DemoOption>();
            _hdVideoStreamArgs = new Mock<HdVideoStreamOption>();
            _navDataArgs = new Mock<NavData.NavData>();
            _navDataArgs.Setup(x => x.Demo).Returns(_demoArgs.Object);
            _navDataArgs.Setup(x => x.HdVideoStream).Returns(_hdVideoStreamArgs.Object);
        }

        private void InitializeFactoryAndWorkerMocksAndConnect()
        {
            InitializeFactoryAndWorkerMocks();
            _target.Connect();
        }

        private void VerifyAllWorkersAndTimersDisposed()
        {
            _mockControlWorker.Verify(x => x.Dispose());
            _mockCommandWorker.Verify(x => x.Dispose());
            _mockVideoWorker.Verify(x => x.Dispose());
            _mockNavDataWorker.Verify(x => x.Dispose());
            _mockCommandTimer.Verify(x => x.Dispose());
        }

        private void VerifyNewWorkersAndTimerRunAndConnectedSetToTrue()
        {
            _mockControlWorker.Verify(x => x.Run());
            _mockNavDataWorker.Verify(x => x.Run());
            _mockCommandWorker.Verify(x => x.Run());
            _target.CommandTimer.Should().Be(_mockCommandTimer.Object);
            VerifyInitialCommandsQueued();
            _target.Connected.Should().BeTrue();
        }

        private void VerifyInitialCommandsQueued()
        {
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GeneralVideoEnableConfigKey, DroneController.TrueConfigValue));
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoBitrateCtrlModeConfigKey, "0"));
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoVideoChannelConfigKey, "0"));
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GeneralNavdataDemo, DroneController.TrueConfigValue));
            _mockCommandWorker.Verify(
                x =>
                    x.SendConfigCommand(DroneController.GeneralNavdataOptionsConfigKey,
                        DroneController.NavDataOptions.ToString()));
            _mockCommandWorker.Verify(
                x => x.ExitBootStrapMode());
        }

        private void InitializeFactoryAndWorkerMocks()
        {
            _mockDispatcher = new Mock<IDispatcher>();
            _mockWorkerFactory = new Mock<WorkerFactory>();
            _mockTimerFactory = new Mock<TimerFactory>();
            _target.TimerFactory = _mockTimerFactory.Object;
            _target.WorkerFactory = _mockWorkerFactory.Object;
            _target.Dispatcher = _mockDispatcher.Object;
            _mockControlWorker = new Mock<ControlWorker>();
            _mockCommandWorker = new Mock<CommandWorker>();
            _mockNavDataWorker = new Mock<NavDataWorker>();
            _mockVideoWorker = new Mock<VideoWorker>();
            _mockCommandTimer = new Mock<IDisposable>();
            _mockWorkerFactory.Setup(x => x.CreateControlWorker())
                              .Returns(_mockControlWorker.Object);
            _mockWorkerFactory.Setup(x => x.CreateCommandWorker())
                              .Returns(_mockCommandWorker.Object);
            _mockWorkerFactory.Setup(x => x.CreateNavDataWorker())
                              .Returns(_mockNavDataWorker.Object);
            _mockTimerFactory.Setup(x => x.CreateTimer())
                              .Returns(_mockCommandTimer.Object);
            _mockDispatcher.Setup(x => x.BeginInvoke(It.IsAny<Action>())).Callback<Action>(a => a.Invoke());
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
            _target.CommWatchDog = true;
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
            _target.CommWatchDog.Should().BeFalse();
        }
    }
}
