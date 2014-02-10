using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const double TestLatitude = 123.456;
        private const double TestLongitude = 234.567;
        private const double TestAltitude = 345.678;
        private const long ConvertedTestLatitude = 123456;
        private const long ConvertedTestLongitude = 234567;
        private const long ConvertedTestAltitude = 345678;
        private const int RecordScreenshotDelayInSecondsTestValue = 337;

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
        private Mock<DoubleToInt64Converter> _mockDoubleToInt64Converter;
        private Mock<DateTimeFactory> _mockDateTimeFactory;
        private DateTime _recordScreenshotDateTimeTestValue;

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
            _target.RecordScreenshotDelayInSeconds.Should().Be(1);
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
            _target.DoubleToInt64Converter.Should().BeOfType<DoubleToInt64Converter>();
            _target.DateTimeFactory.Should().BeOfType<DateTimeFactory>();
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
            VeryifyAllWorkersAndTmersDisposedAndPropertiesSetToDefaults();
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
            VeryifyAllWorkersAndTmersDisposedAndPropertiesSetToDefaults();
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
            _mockDispatcher.Verify(x => x.BeginInvoke(It.IsAny<Action>()));
            VeryifyAllWorkersAndTmersDisposedAndPropertiesSetToDefaults();
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
            RaiseNavDataReceivedEvent(null);

            // Assert
            // no exception thrown when a null eventarg would have otherwise guaranteed an exception
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
        public void CanSendFlatTrimCommandIsOnlyTrueWhenNotFlyingAndConnected()
        {
            _target.Connected = true;
            _target.Flying = false;
            _target.CanSendFlatTrimCommand.Should().BeTrue();

            _target.Connected = true;
            _target.Flying = true;
            _target.CanSendFlatTrimCommand.Should().BeFalse();

            _target.Connected = false;
            _target.Flying = true;
            _target.CanSendFlatTrimCommand.Should().BeFalse();

            _target.Connected = false;
            _target.Flying = false;
            _target.CanSendFlatTrimCommand.Should().BeFalse();
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
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.CommWatchDog).Returns(true);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            _target.CommWatchDog.Should().BeTrue();
        }

        [TestMethod]
        public void NavDataWorkerReceivesNoHdVideoStreamOption_RequestNavDataAgain()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.HdVideoStream).Returns((HdVideoStreamOption)null);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            VerifyNavDataRequestQueued(2);
        }

        [TestMethod]
        public void NavDataWorkerReceivesDemoOption_RequestNavDataAgain()
        {
            // Arrange
            InitializeWorkersAndNavDataEventArgsAndConnect();
            _navDataArgs.Setup(x => x.Demo).Returns((DemoOption)null);

            // Act
            RaiseNavDataReceivedEvent();

            // Assert
            VerifyNavDataRequestQueued(2);
        }

        #endregion

        #region Command Tests

        [TestMethod]
        public void WhenOutdoorIsTrue_TakeOff_SendOutdoorConfigCommand()
        {
            VerifyTakeOffSendsOutdoorConfigCommand(true, DroneController.TrueConfigValue);
        }

        [TestMethod]
        public void WhenOutdoorIsFalse_TakeOff_SendOutdoorConfigCommand()
        {
            VerifyTakeOffSendsOutdoorConfigCommand(false, DroneController.FalseConfigValue);
        }

        [TestMethod]
        public void WhenShellOnIsTrue_TakeOff_SendFlightWithoutShellConfigCommand()
        {
            VerifyTakeOffSendsFlightWithoutShellConfigCommand(true, DroneController.FalseConfigValue);
        }

        [TestMethod]
        public void WhenShellOnIsFalse_TakeOff_SendFlightWithoutShellConfigCommand()
        {
            VerifyTakeOffSendsFlightWithoutShellConfigCommand(false, DroneController.TrueConfigValue);
        }

        [TestMethod]
        public void WhenCombineYawIsTrue_TakeOff_SendControlLevelConfigCommand()
        {
            VerifyTakeOffSendsControlLevelConfigCommand(true, 2);
        }

        [TestMethod]
        public void WhenCombineYawIsFalse_TakeOff_SendControlLevelConfigCommand()
        {
            VerifyTakeOffSendsControlLevelConfigCommand(false, 0);
        }

        [TestMethod]
        public void WhenAltitudeMaxIs13_TakeOff_SendsAltitudeMaxConfigCommand()
        {
            VerifyTakeOffSendsAltitudeMaxConfigCommand(13, 13000);
        }

        [TestMethod]
        public void WhenAltitudeMaxIs19_TakeOff_SendsAltitudeMaxConfigCommand()
        {
            VerifyTakeOffSendsAltitudeMaxConfigCommand(19, 19000);
        }

        [TestMethod]
        public void WhenMaxDeviceTiltIs30_TakeOff_SendsMaxDeviceTiltConfigCommand()
        {
            VerifyTakeOffSendsMaxDeviceTiltConfigCommand(30, 30 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxDeviceTiltIs17_TakeOff_SendsMaxDeviceTiltConfigCommand()
        {
            VerifyTakeOffSendsMaxDeviceTiltConfigCommand(17, 17 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxIndoorYawDegrees30_TakeOff_SendsMaxIndoorYawDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorYawDegreesConfigCommand(30, 30 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxIndoorYawDegrees17_TakeOff_SendsMaxIndoorYawDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorYawDegreesConfigCommand(17, 17 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxOutdoorYawDegrees30_TakeOff_SendsMaxOutdoorYawDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorYawDegreesConfigCommand(30, 30 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxOutdoorYawDegrees17_TakeOff_SendsMaxOutdoorYawDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorYawDegreesConfigCommand(17, 17 * DroneController.DegreesToRadiansCoefficient);
        }

        private void VerifyTakeOffSendsMaxOutdoorYawDegreesConfigCommand(int maxOutdoorYawDegrees, double maxOutdoorYawRadians)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxOutdoorYawDegrees = maxOutdoorYawDegrees;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.OutdoorControlYawConfigKey, maxOutdoorYawRadians.ToString(CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void WhenMaxOutdoorRollOrPitchDegrees30_TakeOff_SendsMaxOutdoorRollOrPitchDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorRollOrPitchDegreesConfigCommand(30, 30 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxOutdoorRollOrPitchDegrees17_TakeOff_SendsMaxOutdoorRollOrPitchDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorRollOrPitchDegreesConfigCommand(17, 17 * DroneController.DegreesToRadiansCoefficient);
        }

        private void VerifyTakeOffSendsMaxOutdoorRollOrPitchDegreesConfigCommand(int maxOutdoorRollOrPitchDegrees, double maxOutdoorRollOrPitchRadians)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxOutdoorRollOrPitchDegrees = maxOutdoorRollOrPitchDegrees;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.OutdoorEulerAngleMaxConfigKey, maxOutdoorRollOrPitchRadians.ToString(CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void WhenMaxIndoorRollOrPitchDegrees30_TakeOff_SendsMaxIndoorRollOrPitchDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorRollOrPitchDegreesConfigCommand(30, 30 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxIndoorRollOrPitchDegrees17_TakeOff_SendsMaxIndoorRollOrPitchDegreesConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorRollOrPitchDegreesConfigCommand(17, 17 * DroneController.DegreesToRadiansCoefficient);
        }

        [TestMethod]
        public void WhenMaxIndoorVerticalCmPerSecond30_TakeOff_SendsMaxIndoorVerticalCmPerSecondConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorVerticalSpeedConfigCommand(30, 300);
        }

        [TestMethod]
        public void WhenMaxIndoorVerticalCmPerSecond17_TakeOff_SendsMaxIndoorVerticalCmPerSecondConfigCommand()
        {
            VerifyTakeOffSendsMaxIndoorVerticalSpeedConfigCommand(17, 170);
        }

        private void VerifyTakeOffSendsMaxIndoorVerticalSpeedConfigCommand(int maxIndoorCmPerSecond, double maxIndoorMmPerSecond)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxIndoorVerticalCmPerSecond = maxIndoorCmPerSecond;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.IndoorMaxVerticalSpeedConfigKey, maxIndoorMmPerSecond.ToString(CultureInfo.InvariantCulture)));
        }

        [TestMethod]
        public void WhenMaxOutdoorVerticalCmPerSecond30_TakeOff_SendsMaxOutdoorVerticalCmPerSecondConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorVerticalSpeedConfigCommand(30, 300);
        }

        [TestMethod]
        public void WhenMaxOutdoorVerticalCmPerSecond17_TakeOff_SendsMaxOutdoorVerticalCmPerSecondConfigCommand()
        {
            VerifyTakeOffSendsMaxOutdoorVerticalSpeedConfigCommand(17, 170);
        }

        [TestMethod]
        public void TakeOff_SendsFlatTrimAndTakeOffCommand()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(x => x.SendFlatTrimCommand());
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
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoCodecConfigKey, DroneController.H264_360P_CodecConfigValue));
            _mockVideoWorker.Verify(x => x.Dispose());
        }

        #endregion

        #region CommandTimer Tick tests

        [TestMethod]
        public void WhenConnected_CommandTimerTick_FlushesCommandWorker()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.Flush());
        }

        [TestMethod]
        public void WhenNotConnected_CommandTimerTick_CommandWorkerIsNeverFlushed()
        {
            // Arrange
            InitializeFactoryAndWorkerMocks();

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.Flush(), Times.Never);
        }

        [TestMethod]
        public void IfExceptionFlushingCommandWorker_CommandTimerTick_Disconnect()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.VideoWorker = _mockVideoWorker.Object;
            _mockCommandWorker.Setup(x => x.Flush()).Throws(new Exception("Test Exception"));

            // Act
            _target.DoWork(null);

            // Assert
            VeryifyAllWorkersAndTmersDisposedAndPropertiesSetToDefaults();
        }

        [TestMethod]
        public void CommandTimerTick_SendsCalibrateCompassCommand5SecondsAfterTakeoff()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            InitializeFactoryAndWorkerMocksAndConnect();
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime);
            _target.TakeOff();

            VerifyCommandTimerSendsCalibrateCompassCommand(startTime, 0, 0);
            VerifyCommandTimerSendsCalibrateCompassCommand(startTime, 5, 0);
            VerifyCommandTimerSendsCalibrateCompassCommand(startTime, 6, 1);
            VerifyCommandTimerSendsCalibrateCompassCommand(startTime, 10, 1);
        }

        private const int ProgressiveCommandTimeDelayInSeconds = 10;

        [TestMethod]
        public void WhenFlyingNotCommWatchDog_CommandTimerTick_SendsProgressiveCommandsDealyedAfterTakeoff()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            InitializeFactoryAndWorkerMocksAndConnect();
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime);
            _target.TakeOff();
            _target.Flying = true;
            _target.CommWatchDog = false;

            VerifyCommandTimerSendsProgressiveCommand(startTime, 0, 0);

            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds, 0);
            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds + 1, 1);
            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds + 2, 2);
            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds + 3, 3);
            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds + 4, 4);
            VerifyCommandTimerSendsProgressiveCommand(startTime, ProgressiveCommandTimeDelayInSeconds + 5, 5);
        }

        [TestMethod]
        public void WhenAfterTakeoffDelayAndNoCommWatchdogAndNoFlying_CommandTimerTick_DoesNotSendProgressiveCommand()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            InitializeFactoryAndWorkerMocksAndConnect();
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime);
            _target.TakeOff();
            _target.Flying = false;
            _target.CommWatchDog = false;
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime.AddSeconds(ProgressiveCommandTimeDelayInSeconds + 1));

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendProgressiveCommand(_target), Times.Never);
        }

        [TestMethod]
        public void WhenAfterTakeoffDelayCommWatchdogAndFlying_CommandTimerTick_DoesNotSendProgressiveCommand()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            InitializeFactoryAndWorkerMocksAndConnect();
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime);
            _target.TakeOff();
            _target.Flying = true;
            _target.CommWatchDog = true;
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime.AddSeconds(ProgressiveCommandTimeDelayInSeconds + 1));

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendProgressiveCommand(_target), Times.Never);
        }

        [TestMethod]
        public void WhenNotCommWatchdog_CommandTimerTick_DoesNotSendResetWatchdogCommand()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.CommWatchDog = false;

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendResetWatchDogCommand(), Times.Never);
        }

        [TestMethod]
        public void WhenCommWatchdog_CommandTimerTick_SendsResetWatchdogCommand()
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.CommWatchDog = true;

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendResetWatchDogCommand());
        }

        #endregion

        #region Settings Tests

        [TestMethod]
        public void Constructor_AssignsDefaultsToAllSettings()
        {
            // Assert
            VerifyAllSettingsAreDefault();
        }

        [TestMethod]
        public void ResetSettings_ResetsAllSettingsToDefaults()
        {
            // Arrange
            AssignNonDefaultsToAllSettings();

            // Act
            _target.ResetSettings();

            // Assert
            VerifyAllSettingsAreDefault();
        }

        #region SendLocationInfo

        [TestMethod]
        public void WhenSendLocationInfoIsTrueAndConnected_SetGps_SetsGpsConfigValues()
        {
            // Arrange
            InitializeFactoryAndWorkerAndConverterMocks();
            _target.Connect();
            _target.CanSendLocationInformation = true;

            // Act
            _target.SetLocation(TestLatitude, TestLongitude, TestAltitude);

            // Assert
            VerifyGpsConfigValuesSent(Times.Once);
        }

        [TestMethod]
        public void WhenSendLocationInfoIsFalse_SetGps_DoesNotSendGpsConfigValues()
        {
            // Arrange
            InitializeFactoryAndWorkerAndConverterMocks();
            _target.Connect();
            _target.CanSendLocationInformation = false;

            // Act
            _target.SetLocation(TestLatitude, TestLongitude, TestAltitude);

            // Assert
            VerifyGpsConfigValuesSent(Times.Never);
        }

        [TestMethod]
        public void WhenNotConnected_SetGps_DoesNotSendGpsConfigValues()
        {
            // Arrange
            InitializeFactoryAndWorkerAndConverterMocks();
            _target.CanSendLocationInformation = true;

            // Act
            _target.SetLocation(TestLatitude, TestLongitude, TestAltitude);

            // Assert
            VerifyGpsConfigValuesSent(Times.Never);
        }

        [TestMethod]
        public void WhenConnected_SendLocationInfoIsSetToTrue_SendsGpsConfigValues()
        {
            // Arrange
            InitializeFactoryAndWorkerAndConverterMocks();
            _target.CanSendLocationInformation = false;
            _target.SetLocation(TestLatitude, TestLongitude, TestAltitude);
            _target.Connect();

            // Act
            _target.CanSendLocationInformation = true;

            // Assert
            VerifyGpsConfigValuesSent(Times.Once);
        }

        [TestMethod]
        public void WhenSendLocationInfoIsSetToTrue_Connect_SendsGpsConfigValues()
        {
            // Arrange
            InitializeFactoryAndWorkerAndConverterMocks();
            _target.CanSendLocationInformation = true;
            _target.SetLocation(TestLatitude, TestLongitude, TestAltitude);

            // Act
            _target.Connect();

            // Assert
            VerifyGpsConfigValuesSent(Times.Once);
        }

        #endregion

        #region RecordFlightData

        [TestMethod]
        public void WhenRecordFlightDataIsTrue_Connect_SendsStartUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = true;

            // Act
            _target.Connect();

            // Assert
            VerifyUserBoxStartAndScreenShotCommandsSent(Times.Once);
            VerifyUserBoxStopCommandSent(Times.Never);
        }

        [TestMethod]
        public void WhenRecordFlightDataIsFalse_Connect_SendsStopUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = false;

            // Act
            _target.Connect();

            // Assert
            VerifyUserBoxStartAndScreenShotCommandsSent(Times.Never);
            VerifyUserBoxStopCommandSent(Times.Once);
        }

        [TestMethod]
        public void WhenConnected_RecordFlightDataIsSetToTrue_SendsStartUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.Connect();

            // Act
            _target.RecordFlightData = true;

            // Assert
            VerifyUserBoxStartAndScreenShotCommandsSent(Times.Once);
        }

        [TestMethod]
        public void WhenConnected_RecordFlightDataIsSetToFalse_SendsStopUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = true;
            _target.Connect();

            // Act
            _target.RecordFlightData = false;

            // Assert
            VerifyUserBoxStopCommandSent(Times.Once);
        }

        [TestMethod]
        public void WhenRecordFlightDataIsTrue_Disconnect_SendsStopUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = true;
            _target.Connect();

            // Act
            _target.Disconnect();

            // Assert
            VerifyUserBoxStopCommandSent(Times.Once);
        }

        [TestMethod]
        public void WhenRecordFlightDataIsTrueAndNotConnected_Disconnect_SendsNoUserBoxStopCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = true;

            // Act
            _target.Disconnect();

            // Assert
            VerifyUserBoxStopCommandSent(Times.Never);
        }

        [TestMethod]
        public void WhenRecordFlightDataIsFalse_Disconnect_DoesNotSendStopUserBoxConfigCommand()
        {
            // Arrange
            ArrangeRecordFlightDataTest();
            _target.RecordFlightData = false;
            _target.Connect();

            // Act
            _target.Disconnect();

            // Assert
            VerifyUserBoxStopCommandSent(Times.Once); // the one time when we connected, and no more
        }

        [TestMethod]
        public void CanSetRecordScreenshotDelayInSecondsTrueIfNotConnectedOrNotRecordFlightData()
        {
            _target.RecordFlightData = true;
            _target.Connected = true;
            _target.CanSetRecordScreenshotDelayInSeconds.Should().BeFalse();

            _target.Connected = false;
            _target.RecordFlightData = false;
            _target.CanSetRecordScreenshotDelayInSeconds.Should().BeTrue();

            _target.RecordFlightData = true;
            _target.Connected = false;
            _target.CanSetRecordScreenshotDelayInSeconds.Should().BeTrue();

            _target.RecordFlightData = false;
            _target.Connected = true;
            _target.CanSetRecordScreenshotDelayInSeconds.Should().BeTrue();
        }

        #endregion

        [TestMethod]
        public void SettingFixedRangePropertiesAreSet()
        {
            // Assert
            _target.MaxAltitudeInMetersMax.Should().Be(100);
            _target.MaxAltitudeInMetersMin.Should().Be(1);
            _target.RecordScreenshotDelayInSecondsMax.Should().Be(60);
            _target.RecordScreenshotDelayInSecondsMin.Should().Be(1);
            _target.MaxDeviceTiltInDegreesMin.Should().Be(5);
            _target.MaxDeviceTiltInDegreesMax.Should().Be(50);
            _target.MaxYawDegreesMax.Should().Be(350);
            _target.MaxYawDegreesMin.Should().Be(40);
            _target.MaxVeritcalCmPerSecondMax.Should().Be(200);
            _target.MaxVeritcalCmPerSecondMin.Should().Be(20);
            _target.MaxRollOrPitchDegreesMax.Should().Be(30);
            _target.MaxRollOrPitchDegreesMin.Should().Be(1);
        }

        [TestMethod]
        public void CanSetMaxAltitudeIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxAltitude.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxAltitude.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetCombineYawIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetCombineYaw.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetCombineYaw.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxDeviceTiltIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxDeviceTiltInDegrees.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxDeviceTiltInDegrees.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetOutdoorIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetOutdoor.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetOutdoor.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetShellOnIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetShellOn.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetShellOn.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxIndoorYawDegreesIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxIndoorYawDegrees.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxIndoorYawDegrees.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxOutdoorYawDegreesIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxOutdoorYawDegrees.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxOutdoorYawDegrees.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxOutdoorRollOrPitchDegreesIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxOutdoorRollOrPitchDegrees.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxOutdoorRollOrPitchDegrees.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxIndoorRollOrPitchDegreesIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxIndoorRollOrPitchDegrees.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxIndoorRollOrPitchDegrees.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxIndoorVerticalCmPerSecondIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxIndoorVerticalCmPerSecond.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxIndoorVerticalCmPerSecond.Should().BeFalse();
        }

        [TestMethod]
        public void CanSetMaxOutdoorVerticalCmPerSecondIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanSetMaxOutdoorVerticalCmPerSecond.Should().BeTrue();

            _target.Flying = true;
            _target.CanSetMaxOutdoorVerticalCmPerSecond.Should().BeFalse();
        }

        [TestMethod]
        public void CanResetSettingsIsTheOppositeOfFlying()
        {
            _target.Flying = false;
            _target.CanResetSettings.Should().BeTrue();

            _target.Flying = true;
            _target.CanResetSettings.Should().BeFalse();
        }

        [TestMethod]
        public void WhenLockDroneDirectionToDeviceDirectionIsFalse_Yaw_IsWhateverItIsSetTo()
        {
            // Arrange
            _target.LockDroneHeadingToDeviceHeading = false;
            const float testYaw = 6543.171f;

            // Act
            _target.Yaw = testYaw;

            // Assert
            _target.Yaw.Should().Be(testYaw);
        }

        [TestMethod]
        public void WhenDroneDirectionLockedToDeviceDirection_Yaw_IsTheResultofComparingDeviceHeadingToDroneHeading()
        {
            // Arrange
            _target.LockDroneHeadingToDeviceHeading = true;
            const float testYaw = 6543.171f;
            _target.Yaw = testYaw;

            // Act
            VerifyYawFromHeadingAndPsi(0, 0, 0);
            VerifyYawFromHeadingAndPsi(-4.9f, 0, 0f);
            VerifyYawFromHeadingAndPsi(4.9f, 0, 0f);
            VerifyYawFromHeadingAndPsi(-163f, 0f, .9f);
            VerifyYawFromHeadingAndPsi(-90f, 0f, .5f);
            VerifyYawFromHeadingAndPsi(163f, 0f, -.9f);
            VerifyYawFromHeadingAndPsi(90f, 0f, -.5f);
            VerifyYawFromHeadingAndPsi(0f, 163f, .9f);
            VerifyYawFromHeadingAndPsi(0f, 90f, .5f);
            VerifyYawFromHeadingAndPsi(0f, -90f, -.5f);
            VerifyYawFromHeadingAndPsi(0f, -163f, -.9f);
            VerifyYawFromHeadingAndPsi(135f, -135f, .5f);
            VerifyYawFromHeadingAndPsi(-135f, 135f, -.5f);
            VerifyYawFromHeadingAndPsi(-135f, 90f, -.75f);
            VerifyYawFromHeadingAndPsi(135f, -90f, .75f);
            VerifyYawFromHeadingAndPsi(180f, -90f, .5f);
            VerifyYawFromHeadingAndPsi(-180f, 90f, -.5f);
            VerifyYawFromHeadingAndPsi(90f, -180f, .5f);
            VerifyYawFromHeadingAndPsi(-90f, 180f, -.5f);
        }

        private void VerifyYawFromHeadingAndPsi(float psi, float controllerHeading, float yaw)
        {
            _target.ControllerHeading = controllerHeading;
            _target.Psi = psi;
            _target.Yaw.Should().BeApproximately(yaw, 0.01f);
        }

        #endregion

        private void VerifyCommandTimerSendsProgressiveCommand(DateTime startTime, int secondsFromTakeoff,
            int sendProgressiveCommandCallCount)
        {
            // Arrange
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime.AddSeconds(secondsFromTakeoff));

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendProgressiveCommand(_target),
                Times.Exactly(sendProgressiveCommandCallCount));
        }

        private void VerifyCommandTimerSendsCalibrateCompassCommand(DateTime startTime, int secondsFromTakeOff,
            int calibrateCompassCallCount)
        {
            // Arrange
            _mockDateTimeFactory.Setup(x => x.Now).Returns(startTime.AddSeconds(secondsFromTakeOff));

            // Act
            _target.DoWork(null);

            // Assert
            _mockCommandWorker.Verify(x => x.SendCalibrateCompassCommand(), Times.Exactly(calibrateCompassCallCount));
        }

        private void VerifyTakeOffSendsMaxIndoorYawDegreesConfigCommand(int maxIndoorYawDegrees, double maxIndoorYawRadians)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxIndoorYawDegrees = maxIndoorYawDegrees;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.IndoorControlYawConfigKey, maxIndoorYawRadians.ToString(CultureInfo.InvariantCulture)));
        }

        private void VerifyTakeOffSendsMaxIndoorRollOrPitchDegreesConfigCommand(int maxIndoorRollOrPitchDegrees, double maxIndoorRollOrPitchRadians)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxIndoorRollOrPitchDegrees = maxIndoorRollOrPitchDegrees;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.IndoorEulerAngleMaxConfigKey, maxIndoorRollOrPitchRadians.ToString(CultureInfo.InvariantCulture)));
        }

        private void VerifyTakeOffSendsMaxOutdoorVerticalSpeedConfigCommand(int maxOutdoorCmPerSecond, double maxOutdoorMmPerSecond)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxOutdoorVerticalCmPerSecond = maxOutdoorCmPerSecond;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.OutdoorMaxVerticalSpeedConfigKey, maxOutdoorMmPerSecond.ToString(CultureInfo.InvariantCulture)));
        }

        private void VeryifyAllWorkersAndTmersDisposedAndPropertiesSetToDefaults()
        {
            _mockControlWorker.Verify(x => x.Dispose());
            _mockCommandWorker.Verify(x => x.Dispose());
            _mockVideoWorker.Verify(x => x.Dispose());
            _mockNavDataWorker.Verify(x => x.Dispose());
            _mockCommandTimer.Verify(x => x.Dispose());
            AssertPropertiesAreSetToDefaults();
        }

        private void VerifyTakeOffSendsOutdoorConfigCommand(bool outdoor, string expectedConfigValue)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.Outdoor = outdoor;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.OutdoorConfigKey, expectedConfigValue));
        }

        private void VerifyTakeOffSendsFlightWithoutShellConfigCommand(bool shell, string expectedConfigValue)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.ShellOn = shell;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.FlightWithoutShellConfigKey, expectedConfigValue));
        }

        private void VerifyTakeOffSendsAltitudeMaxConfigCommand(int altitudeInMetersTestValue, int altitudeInMillimeters)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxAltitudeInMeters = altitudeInMetersTestValue;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.AltitudeMaxConfigKey, altitudeInMillimeters.ToString(CultureInfo.InvariantCulture)));
        }

        private void VerifyTakeOffSendsMaxDeviceTiltConfigCommand(int maxDeviceTiltTestValue, double maxDeviceTiltInRadians)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.MaxDeviceTiltInDegrees = maxDeviceTiltTestValue;

            // Act
            _target.TakeOff();

            // Assert
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.MaxDeviceTiltConfigKey, maxDeviceTiltInRadians.ToString(CultureInfo.InvariantCulture)));
        }

        private void VerifyTakeOffSendsControlLevelConfigCommand(bool combineYaw, int expectedConfigValue)
        {
            // Arrange
            InitializeFactoryAndWorkerMocksAndConnect();
            _target.CombineYaw = combineYaw;

            // Act
            _target.TakeOff();

            // Assert

            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.ControlLevelConfigKey, expectedConfigValue.ToString(CultureInfo.InvariantCulture)));
        }

        private void ArrangeRecordFlightDataTest()
        {
            InitializeFactoryAndWorkerMocks();
            _target.RecordScreenshotDelayInSeconds = RecordScreenshotDelayInSecondsTestValue;
            _recordScreenshotDateTimeTestValue = new DateTime(1999, 05, 09, 13, 14, 15);
            _mockDateTimeFactory.Setup(x => x.Now).Returns(_recordScreenshotDateTimeTestValue);
        }

        private void VerifyUserBoxStopCommandSent(Func<Times> times)
        {
            _mockCommandWorker.Verify(
                x =>
                    x.SendConfigCommand(DroneController.UserboxConfigKey,
                        ((int)DroneController.UserBoxCommands.Stop).ToString(CultureInfo.InvariantCulture)), times);
        }

        private void VerifyUserBoxStartAndScreenShotCommandsSent(Func<Times> times)
        {
            const uint maxNumberOfScreenshots = 86400;
            string userboxStartCommand = string.Format("{0},{1}", (int)DroneController.UserBoxCommands.Start,
                _recordScreenshotDateTimeTestValue.ToString(DroneController.UserBoxCommandDateFormat));
            string userboxScreenShotCommand = string.Format("{0},{1},{2},{3}",
                (int)DroneController.UserBoxCommands.ScreenShot,
                RecordScreenshotDelayInSecondsTestValue, maxNumberOfScreenshots,
                _recordScreenshotDateTimeTestValue.ToString(DroneController.UserBoxCommandDateFormat));
            _mockCommandWorker.Verify(x => x.SendConfigCommand(DroneController.UserboxConfigKey, userboxStartCommand),
                times);
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.UserboxConfigKey, userboxScreenShotCommand), times);
        }

        private void VerifyRecordCommandSentAndVideoWorkerStarted()
        {
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.VideoOnUsbConfigKey, DroneController.TrueConfigValue));
            _mockCommandWorker.Verify(
                x =>
                x.SendConfigCommand(DroneController.VideoCodecConfigKey, DroneController.Mp4_360p_H264_720p_CodecConfigValue));
            _mockVideoWorker.Verify(x => x.Run());
        }

        private void RaiseNavDataReceivedEvent()
        {
            RaiseNavDataReceivedEvent(_navDataArgs.Object);
        }

        private void RaiseNavDataReceivedEvent(NavData.NavData navData)
        {
            _mockNavDataWorker.Raise(x => x.NavDataReceived += null, new NavDataReceivedEventArgs(navData));
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
            VerifyNavDataRequestQueued(1);
            _mockCommandWorker.Verify(x => x.ExitBootStrapMode());
        }

        private void VerifyNavDataRequestQueued(int numberOfTimes)
        {
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GeneralNavdataDemo, DroneController.TrueConfigValue),
                Times.Exactly(numberOfTimes));
            _mockCommandWorker.Verify(
                x =>
                    x.SendConfigCommand(DroneController.GeneralNavdataOptionsConfigKey,
                        DroneController.NavDataOptions.ToString(CultureInfo.InvariantCulture)), Times.Exactly(numberOfTimes));
        }

        private void InitializeFactoryAndWorkerMocks()
        {
            _mockDispatcher = new Mock<IDispatcher>();
            _mockWorkerFactory = new Mock<WorkerFactory>();
            _mockTimerFactory = new Mock<TimerFactory>();
            _mockDoubleToInt64Converter = new Mock<DoubleToInt64Converter>();
            _mockDateTimeFactory = new Mock<DateTimeFactory>();
            _target.TimerFactory = _mockTimerFactory.Object;
            _target.WorkerFactory = _mockWorkerFactory.Object;
            _target.Dispatcher = _mockDispatcher.Object;
            _target.DoubleToInt64Converter = _mockDoubleToInt64Converter.Object;
            _target.DateTimeFactory = _mockDateTimeFactory.Object;
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
            _target.Flying.Should().BeFalse();
            _target.CanRecord.Should().BeFalse();
            _target.UsbKeyIsRecording.Should().BeFalse();
            _target.CommWatchDog.Should().BeFalse();
        }

        private void InitializeFactoryAndWorkerAndConverterMocks()
        {
            InitializeFactoryAndWorkerMocks();
            _mockDoubleToInt64Converter.Setup(x => x.Convert(TestLatitude)).Returns(ConvertedTestLatitude);
            _mockDoubleToInt64Converter.Setup(x => x.Convert(TestLongitude)).Returns(ConvertedTestLongitude);
            _mockDoubleToInt64Converter.Setup(x => x.Convert(TestAltitude)).Returns(ConvertedTestAltitude);
        }

        private void VerifyGpsConfigValuesSent(Func<Times> times)
        {
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GpsLatitudeConfigKey, ConvertedTestLatitude.ToString(CultureInfo.InvariantCulture)), times);
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GpsLongitudeConfigCommand, ConvertedTestLongitude.ToString(CultureInfo.InvariantCulture)), times);
            _mockCommandWorker.Verify(
                x => x.SendConfigCommand(DroneController.GpsAltitudeConfigCommand, ConvertedTestAltitude.ToString(CultureInfo.InvariantCulture)), times);
        }

        private void VerifyAllSettingsAreDefault()
        {
            _target.CombineYaw.Should().BeFalse();
            _target.AbsoluteControlMode.Should().BeFalse();
            _target.CanSendLocationInformation.Should().BeFalse();
            _target.RecordFlightData.Should().BeFalse();
            _target.RecordScreenshotDelayInSeconds.Should().Be(1);
            _target.MaxAltitudeInMeters.Should().Be(3);
            _target.MaxDeviceTiltInDegrees.Should().Be(20);
            _target.Outdoor.Should().BeFalse();
            _target.ShellOn.Should().BeTrue();
            _target.MaxIndoorYawDegrees.Should().Be(100);
            _target.MaxOutdoorYawDegrees.Should().Be(200);
            _target.MaxIndoorRollOrPitchDegrees.Should().Be(12);
            _target.MaxOutdoorRollOrPitchDegrees.Should().Be(20);
            _target.MaxIndoorVerticalCmPerSecond.Should().Be(70);
            _target.MaxOutdoorVerticalCmPerSecond.Should().Be(100);
        }

        private void AssignNonDefaultsToAllSettings()
        {
            _target.CombineYaw = true;
            _target.AbsoluteControlMode = true;
            _target.CanSendLocationInformation = true;
            _target.RecordFlightData = true;
            _target.RecordScreenshotDelayInSeconds = 13;
            _target.MaxAltitudeInMeters = 15;
            _target.MaxDeviceTiltInDegrees = 17;
            _target.Outdoor = true;
            _target.ShellOn = false;
            _target.MaxIndoorYawDegrees = 23;
            _target.MaxOutdoorYawDegrees = 29;
            _target.MaxIndoorRollOrPitchDegrees = 31;
            _target.MaxOutdoorRollOrPitchDegrees = 37;
            _target.MaxIndoorVerticalCmPerSecond = 41;
            _target.MaxOutdoorVerticalCmPerSecond = 43;
        }
    }
}
