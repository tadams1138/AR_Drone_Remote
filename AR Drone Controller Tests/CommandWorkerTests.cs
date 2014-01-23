using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class CommandWorkerTests
    {
        private CommandWorker _target;
        private Mock<IUdpSocket> _mockUdpSocket;
        private Mock<CommandQueue> _mockCommandQueue;
        private Mock<CommandFormatter> _mockCommandFormatter;
        private Mock<FloatToInt32Converter> _mockFloatToInt32Converter;
        private Mock<ProgressiveCommandFormatter> _mockProgressiveCommandFormatter;
        private Mock<ThreadSleeper> _mockThreadSleeper;
        
        [TestInitialize]
        public void InitializeTests()
        {
            _mockCommandQueue = new Mock<CommandQueue>();
            _mockCommandFormatter = new Mock<CommandFormatter>();
            _mockUdpSocket = new Mock<IUdpSocket>();
            _mockFloatToInt32Converter = new Mock<FloatToInt32Converter>();
            _mockProgressiveCommandFormatter = new Mock<ProgressiveCommandFormatter>();
            _mockThreadSleeper = new Mock<ThreadSleeper>();
            _target = new CommandWorker
            {
                Socket = _mockUdpSocket.Object,
                CommandQueue = _mockCommandQueue.Object,
                CommandFormatter = _mockCommandFormatter.Object,
                FloatToInt32Converter = _mockFloatToInt32Converter.Object,
                ProgressiveCommandFormatter = _mockProgressiveCommandFormatter.Object,
                ThreadSleeper = _mockThreadSleeper.Object
            };
        }

        [TestMethod]
        public void Constructor_AssignsThreadSleeper()
        {
            // Arrange

            // Act
            var cw = new CommandWorker();

            // Assert
            cw.ThreadSleeper.Should().BeOfType<ThreadSleeper>();
        }

        [TestMethod]
        public void Run_CallsSocketConnectAndEnqueuesInitialCommands()
        {
            // Arrange
            const string pmodeCommand = "pmodeCommand";
            const string miscCommand = "miscCommand";
            const string configIdsCommand = "configIdsCommand";
            const string sessionIdConfigCommand = "sessionIdConfigCommand";
            const string profileIdConfigCommand = "profileIdConfigCommand";
            const string applicationIdConfigCommand = "applicationIdConfigCommand";
            const string applicationDescConfigCommand = "applicationDescConfigCommand";
            const string profileDescConfigCommand = "profileDescConfigCommand";
            const string sessionDescConfigCommand = "sessionDescConfigCommand";
            const string expectedConfigCommand =
                configIdsCommand + sessionIdConfigCommand + configIdsCommand + profileIdConfigCommand +
                configIdsCommand + applicationIdConfigCommand + configIdsCommand +
                applicationDescConfigCommand + configIdsCommand + profileDescConfigCommand +
                configIdsCommand + sessionDescConfigCommand;
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.PmodeCommand, "2")).Returns(pmodeCommand);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.MiscCommand, "2,20,2000,3000")).Returns(miscCommand);

            SetupCommandFormatterForConfigIdsCommand(configIdsCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.SessionIdConfigKey, _target.SessionId, sessionIdConfigCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.ProfileIdConfigKey, _target.ProfileId, profileIdConfigCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.ApplicationIdConfigKey, _target.ApplicationId, applicationIdConfigCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.ApplicationDescConfigKey, "AR Drone Remote", applicationDescConfigCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.ProﬁleDescConfigKey, ".Primary Profile", profileDescConfigCommand);
            SetupCommandFormatterForConfigCommand(CommandWorker.SessionDescConfigKey, "Session " + _target.SessionId, sessionDescConfigCommand);
            
            // Act
            _target.Run();

            // Assert
            _mockUdpSocket.Verify(x => x.Connect());
            _mockCommandQueue.Verify(x => x.Enqueue(pmodeCommand));
            _mockCommandQueue.Verify(x => x.Enqueue(miscCommand));
            _mockCommandQueue.Verify(x => x.Enqueue(expectedConfigCommand));
        }

        [TestMethod]
        public void WhenCommandsOnQueue_Dispose_SendRemainderOfQueuePauseDisposesSocket()
        {
            // Arrange
            var commands = new Queue<string>();
            const string testCommand1 = "testCommand1";
            commands.Enqueue(testCommand1);
            const string testCommand2 = "testCommand2";
            commands.Enqueue(testCommand2);
            commands.Enqueue(null);
            _mockCommandQueue.Setup(x => x.Flush()).Returns(commands.Dequeue);
            
            // Act 
            _target.Dispose();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(testCommand1));
            _mockUdpSocket.Verify(x => x.Write(testCommand2));
            _mockThreadSleeper.Verify(x => x.Sleep(It.IsAny<int>()));
            _mockUdpSocket.Verify(x => x.Dispose());
        }

        [TestMethod]
        public void WhenNoCommandsOnQueue_Dispose_DisposesSocket()
        {
            // Arrange
            DateTime timeOfLastTransmission =
                DateTime.UtcNow.AddMilliseconds(-1 - CommandWorker.MinMillisecondsSinceLastTransmission);
            _target.TimeOfLastTransmission = timeOfLastTransmission;

            // Act 
            _target.Dispose();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(It.IsAny<string>()), Times.Never);
            _mockThreadSleeper.Verify(x => x.Sleep(It.IsAny<int>()), Times.Never);
            _mockUdpSocket.Verify(x => x.Dispose());
        }

        [TestMethod]
        public void WhenTimeOfLastTransmissionTooRecent_Dispose_PausesBeforeSocketDisposal()
        {
            // Arrange
            DateTime timeOfLastTransmission = DateTime.UtcNow.AddMilliseconds(-1);
            _target.TimeOfLastTransmission = timeOfLastTransmission;

            // Act 
            _target.Dispose();

            // Assert
            _mockThreadSleeper.Verify(x => x.Sleep(It.IsAny<int>()));
            _mockUdpSocket.Verify(x => x.Dispose());
        }
        
        [TestMethod]
        public void GivenResultFromCommandFlush_Flush_SendsCommandToSocket()
        {
            // Arrange
            const string testCommand = "testCommand";
            _mockCommandQueue.Setup(x => x.Flush()).Returns(testCommand);
            DateTime timeOfLastTransmission = DateTime.UtcNow.AddMilliseconds(-1);
            _target.TimeOfLastTransmission = timeOfLastTransmission;

            // Act
            _target.Flush();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(testCommand));
            _target.TimeOfLastTransmission.Should().BeAfter(timeOfLastTransmission);
        }

        [TestMethod]
        public void GivenNullResultFromCommandFlush_Flush_SendsNoCommandToSocket()
        {
            // Arrange
            const string testCommand = null;
            _mockCommandQueue.Setup(x => x.Flush()).Returns(testCommand);
            DateTime timeOfLastTransmission = DateTime.UtcNow.AddMilliseconds(-1);
            _target.TimeOfLastTransmission = timeOfLastTransmission;

            // Act
            _target.Flush();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(It.IsAny<string>()), Times.Never());
            _target.TimeOfLastTransmission.Should().Be(timeOfLastTransmission);
        }

        [TestMethod]
        public void GivenNullResultFromCommandFlushAndInactivityThresholdExceeded_Flush_SendsAckToSocket()
        {
            // Arrange
            const string testCommand = null;
            _mockCommandQueue.Setup(x => x.Flush()).Returns(testCommand);
            DateTime timeOfLastTransmission = DateTime.UtcNow.AddMilliseconds(-CommandWorker.MaxMillisecondsOfInactivity - 1);
            _target.TimeOfLastTransmission = timeOfLastTransmission;

            const string testAck = "testAck";
            SetupCommandFormatterForAck(testAck);

            // Act
            _target.Flush();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(testAck));
            _target.TimeOfLastTransmission.Should().BeAfter(timeOfLastTransmission);
        }

        [TestMethod]
        public void GivenNullResultFromFirstCommandFlush_Flush_SendsNoCommandToSocket()
        {
            // Arrange
            const string testCommand = null;
            _mockCommandQueue.Setup(x => x.Flush()).Returns(testCommand);

            // Act
            _target.Flush();

            // Assert
            _mockUdpSocket.Verify(x => x.Write(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void SendFlatTrim_EnqueuesFlatTrimCommand()
        {
            // Arrange 
            const string testFormattedCommand = "testFormattedCommand";
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.FtrimCommand)).Returns(testFormattedCommand);

            // Act
            _target.SendFlatTrimCommand();

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        [TestMethod]
        public void SendConfigCommand_EnqueuesConfigCommand()
        {
            // Arrange 
            const string testFormattedCommand1 = "testFormattedCommand1";
            const string testFormattedCommand2 = "testFormattedCommand2";
            const string testKey = "testKey";
            const string testValue = "testValue";

            SetupCommandFormatterForConfigIdsCommand(testFormattedCommand1);
            SetupCommandFormatterForConfigCommand(testKey, testValue, testFormattedCommand2);

            // Act
            _target.SendConfigCommand(testKey, testValue);

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand1 + testFormattedCommand2));
        }

        [TestMethod]
        public void SendLandOrResetCommand_EnqueuesRefCommand()
        {
            TestRefCommand(CommandWorker.RefCommands.LandOrReset, "testFormattedLandOrResetCommand");
        }

        [TestMethod]
        public void SendEmergencyCommand_EnqueuesRefCommand()
        {
            TestRefCommand(CommandWorker.RefCommands.Emergency, "testFormattedEmergencyCommand");
        }

        [TestMethod]
        public void SendTakeOffCommand_EnqueuesRefCommand()
        {
            TestRefCommand(CommandWorker.RefCommands.TakeOff, "testFormattedTakeOffCommand");
        }

        [TestMethod]
        public void SendLedAnimationCommand_EnqueuesLedAnimationCommand()
        {
            // Arrange 
            const string testFormattedCommand = "testFormattedLedCommand";
            var mockLedAnimation = CreateMockLedAnimation(LedAnimations.SnakeGreenRed, 11, 22, 3.3f);            
            string message = string.Format("{0},{1},{2}", (int) LedAnimations.SnakeGreenRed, 11, 22);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.LedCommand, message)).Returns(testFormattedCommand);

            // Act
            _target.SendLedAnimationCommand(mockLedAnimation.Object);

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        [TestMethod]
        public void SendFlightAnimationCommand_EnqueuesFlightAnimationCommand()
        {
            // Arrange 
            var mockFlightAnimation = CreateMockFlightAnimation(FlightAnimations.Turnaround, 23);
            var message = string.Format("{0},{1}", (int) FlightAnimations.Turnaround, 23);
            const string testFormattedCommand = "testFormattedFlightCommand";
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.AnimCommand, message)).Returns(testFormattedCommand);

            // Act
            _target.SendFlightAnimationCommand(mockFlightAnimation.Object);

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        private static Mock<IFlightAnimation> CreateMockFlightAnimation(FlightAnimations animation, int timeout)
        {
            var mockFlightAnimation = new Mock<IFlightAnimation>();
            mockFlightAnimation.Setup(x => x.Animation).Returns(animation);
            mockFlightAnimation.Setup(x => x.MaydayTimeoutInMilliseconds).Returns(timeout);
            return mockFlightAnimation;
        }

        [TestMethod]
        public void ExitBootStrapMode_EnqueuesNavdataConfigAndAck()
        {
            // Arrange 
            const string testAck = "testAck";
            const string testFormattedCommand1 = "testFormattedCommand1";
            const string testFormattedCommand2 = "testFormattedCommand2";
            SetupCommandFormatterForConfigIdsCommand(testFormattedCommand1);
            SetupCommandFormatterForConfigCommand(CommandWorker.GeneralNavdataDemoConfigKey,
                CommandWorker.TrueConfigValue, testFormattedCommand2);

            SetupCommandFormatterForAck(testAck);

            // Act
            _target.ExitBootStrapMode();

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand1 + testFormattedCommand2));
            _mockCommandQueue.Verify(x => x.Enqueue(testAck));
        }

        [TestMethod]
        public void CalibrateCompass_EnqueuesCalibrateCommand()
        {
            // Arrange
            const string testFormattedCommand = "testFormattedCommand";
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.CalibCommand, "0"))
                .Returns(testFormattedCommand);

            // Act
            _target.SendCalibrateCompassCommand();

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        [TestMethod]
        public void ResetWatchDog_EnqueuesResetCommand()
        {
            // Arrange
            const string testFormattedCommand = "testFormattedCommand";
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.ComwdgCommand))
                .Returns(testFormattedCommand);

            // Act
            _target.SendResetWatchDogCommand();

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        [TestMethod]
        public void GivenRelativeControl_SendProgressiveCommand_EnqueuesProgressiveCommand()
        {
            // Arrange
            const string testFormattedCommand = "testFormattedCommand";
            const bool absoluteControl = false;
            var mockProgressiveCommand = CreateMockProgressiveCommand(absoluteControl);

            const ProgressiveCommandFormatter.Modes mode = ProgressiveCommandFormatter.Modes.CombineYaw;
            const int roll = 202;
            const int pitch = 303;
            const int gaz = 404;
            const int yaw = 505;
            SetupModeRollPitchGazAndYaw(mode, roll, pitch, gaz, yaw);

            string message = string.Format("{0},{1},{2},{3},{4}", (int)mode, roll, pitch, gaz, yaw);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.PcmdCommand, message))
                .Returns(testFormattedCommand);

            // Act
            _target.SendProgressiveCommand(mockProgressiveCommand.Object);

            // Assert
            _mockProgressiveCommandFormatter.Verify(x => x.Load(mockProgressiveCommand.Object));
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        [TestMethod]
        public void GivenAbsoluteControl_SendProgressiveCommand_EnqueuesProgressiveCommand()
        {
            // Arrange
            const string testFormattedCommand = "testFormattedCommand";
            const bool absoluteControl = true;
            var mockProgressiveCommand = CreateMockProgressiveCommand(absoluteControl);

            const ProgressiveCommandFormatter.Modes mode = ProgressiveCommandFormatter.Modes.CombineYaw;
            const int roll = 202;
            const int pitch = 303;
            const int gaz = 404;
            const int yaw = 505;
            const int magnetoPsi = 606;
            const int magnetoPsiAccuracy = 707;
            SetupModeRollPitchGazAndYaw(mode, roll, pitch, gaz, yaw);
            SetupMagnetoPsiAndMagnetoPsiAccuracy(magnetoPsi, magnetoPsiAccuracy);

            string message = string.Format("{0},{1},{2},{3},{4},{5},{6}", (int)mode, roll, pitch, gaz, yaw, magnetoPsi,
                magnetoPsiAccuracy);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.PcmdMagCommand, message))
                .Returns(testFormattedCommand);

            // Act
            _target.SendProgressiveCommand(mockProgressiveCommand.Object);

            // Assert
            _mockProgressiveCommandFormatter.Verify(x => x.Load(mockProgressiveCommand.Object));
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }

        private void SetupMagnetoPsiAndMagnetoPsiAccuracy(int magnetoPsi, int magnetoPsiAccuracy)
        {
            _mockProgressiveCommandFormatter.Setup(x => x.MagnetoPsi).Returns(magnetoPsi);
            _mockProgressiveCommandFormatter.Setup(x => x.MagnetoPsiAccuracy).Returns(magnetoPsiAccuracy);
        }

        private void SetupModeRollPitchGazAndYaw(ProgressiveCommandFormatter.Modes mode, int roll, int pitch, int gaz, int yaw)
        {
            _mockProgressiveCommandFormatter.Setup(x => x.Mode).Returns(mode);
            _mockProgressiveCommandFormatter.Setup(x => x.Roll).Returns(roll);
            _mockProgressiveCommandFormatter.Setup(x => x.Pitch).Returns(pitch);
            _mockProgressiveCommandFormatter.Setup(x => x.Gaz).Returns(gaz);
            _mockProgressiveCommandFormatter.Setup(x => x.Yaw).Returns(yaw);
        }

        private static Mock<IProgressiveCommand> CreateMockProgressiveCommand(bool absoluteControl)
        {
            var mockProgressiveCommand = new Mock<IProgressiveCommand>();
            mockProgressiveCommand.Setup(x => x.AbsoluteControlMode).Returns(absoluteControl);
            return mockProgressiveCommand;
        }

        private Mock<ILedAnimation> CreateMockLedAnimation(LedAnimations command, int freq, int duration,
            float freqAsFloat)
        {
            var result = new Mock<ILedAnimation>();
            result.Setup(x => x.Animation).Returns(command);
            result.Setup(x => x.FrequencyInHz).Returns(freqAsFloat);
            result.Setup(x => x.DurationInSeconds).Returns(duration);
            _mockFloatToInt32Converter.Setup(x => x.Convert(freqAsFloat)).Returns(freq);
            return result;
        }

        private void SetupCommandFormatterForConfigCommand(string testKey, string testValue, string testFormattedCommand)
        {
            string configMessage = string.Format("\"{0}\",\"{1}\"", testKey, testValue);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.ConfigCommand, configMessage))
                .Returns(testFormattedCommand);
        }

        private void SetupCommandFormatterForConfigIdsCommand(string testFormattedCommand)
        {
            string configIdsMessage = string.Format("\"{0}\",\"{1}\",\"{2}\"", _target.SessionId, _target.ProfileId,
                _target.ApplicationId);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.ConfigIdsCommand, configIdsMessage))
                .Returns(testFormattedCommand);
        }

        private void SetupCommandFormatterForAck(string testAck)
        {
            string ackMessage = string.Format("{0},0", (int)CommandWorker.ControlMode.NoControlMode);
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.CtrlCommand, ackMessage)).Returns(testAck);
        }

        private void TestRefCommand(CommandWorker.RefCommands command, string testFormattedCommand)
        {
            // Arrange
            _mockCommandFormatter.Setup(x => x.CreateCommand(CommandWorker.RefCommand, ((int)command).ToString()))
                .Returns(testFormattedCommand);

            // Act
            _target.SendRefCommand(command);

            // Assert
            _mockCommandQueue.Verify(x => x.Enqueue(testFormattedCommand));
        }
    }
}
