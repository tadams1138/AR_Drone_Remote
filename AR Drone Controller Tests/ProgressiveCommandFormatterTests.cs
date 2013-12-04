using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class ProgressiveCommandFormatterTests
    {
        private ProgressiveCommandFormatter _target;
        private Mock<IProgressiveCommand> _mockProgressiveCommand;
        private Mock<FloatToInt32Converter> _mockFloatToInt32Converter;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockFloatToInt32Converter = new Mock<FloatToInt32Converter>();
            _target = new ProgressiveCommandFormatter
            {
                FloatToInt32Converter = _mockFloatToInt32Converter.Object
            };
            _mockProgressiveCommand = new Mock<IProgressiveCommand>();

            AssignUnexpectedValuesToProperties();
        }

        private void AssignUnexpectedValuesToProperties()
        {
            _target.Gaz = -999;
            _target.MagnetoPsi = -999;
            _target.MagnetoPsiAccuracy = -999;
            _target.Pitch = -999;
            _target.Roll = -999;
            _target.Yaw = -999;
        }

        [TestMethod]
        public void Constructor_AssignsFloatToInt32Converter()
        {
            // Arrange

            // Act
            var result = new ProgressiveCommandFormatter();

            // Assert
            result.FloatToInt32Converter.Should().BeOfType<FloatToInt32Converter>();
        }

        [TestMethod]
        public void GivenPitchAndRollWithinThreshold_Load_SetsPitchAndRollTo0AndEnableProgressiveCommandsNotSetInMode()
        {
            // Arrange
            const float pitch = ProgressiveCommandFormatter.Threshold / 2;
            const float roll = ProgressiveCommandFormatter.Threshold / 2;
            TestPitchAndRollUnderThreshold(pitch, roll);
            TestPitchAndRollUnderThreshold(-pitch, roll);
            TestPitchAndRollUnderThreshold(pitch, -roll);
            TestPitchAndRollUnderThreshold(-pitch, -roll);
        }

        private void TestPitchAndRollUnderThreshold(float pitch, float roll)
        {
            _mockProgressiveCommand.Setup(x => x.Pitch).Returns(pitch);
            _mockProgressiveCommand.Setup(x => x.Roll).Returns(roll);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Pitch.Should().Be(0);
            _target.Roll.Should().Be(0);
            (_target.Mode & ProgressiveCommandFormatter.Modes.EnableProgressiveCommands).Should().Be((ProgressiveCommandFormatter.Modes)0);
        }

        [TestMethod]
        public void GivenPitchGreaterThanOrEqualToThreshold_Load_PitchConvertedToInt32AndEnableProgressiveCommandsSetInMode()
        {
            // Arrange
            const float pitch = ProgressiveCommandFormatter.Threshold;
            _mockProgressiveCommand.Setup(x => x.Pitch).Returns(pitch);
            _mockFloatToInt32Converter.Setup(x => x.Convert(pitch)).Returns(101);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Pitch.Should().Be(101);
            (_target.Mode & ProgressiveCommandFormatter.Modes.EnableProgressiveCommands).Should().NotBe(0);
        }

        [TestMethod]
        public void GivenRollGreaterThanOrEqualToThreshold_Load_RollConvertedToInt32AndEnableProgressiveCommandsSetInMode()
        {
            // Arrange
            const float roll = ProgressiveCommandFormatter.Threshold;
            _mockProgressiveCommand.Setup(x => x.Roll).Returns(roll);
            _mockFloatToInt32Converter.Setup(x => x.Convert(roll)).Returns(101);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Roll.Should().Be(101);
            (_target.Mode & ProgressiveCommandFormatter.Modes.EnableProgressiveCommands).Should()
                .NotBe((ProgressiveCommandFormatter.Modes)0);
        }

        [TestMethod]
        public void Load_GazConvertedToInt32()
        {
            // Arrange
            const float gaz = 1.01f;
            _mockProgressiveCommand.Setup(x => x.Gaz).Returns(gaz);
            _mockFloatToInt32Converter.Setup(x => x.Convert(gaz)).Returns(202);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Gaz.Should().Be(202);
        }

        [TestMethod]
        public void Load_YawConvertedToInt32()
        {
            // Arrange
            const float yaw = 1.01f;
            _mockProgressiveCommand.Setup(x => x.Yaw).Returns(yaw);
            _mockFloatToInt32Converter.Setup(x => x.Convert(yaw)).Returns(202);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Yaw.Should().Be(202);
        }

        [TestMethod]
        public void GivenRelativeControlNoCombinedYawAndPitchAndRollAre0_Load_ModeSetTo0()
        {
            // Arrange
            _target.Mode = ProgressiveCommandFormatter.Modes.CombineYaw;
    
            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.Mode.Should().Be((ProgressiveCommandFormatter.Modes) 0);
        }

        [TestMethod]
        public void GivenCombinedYaw_Load_CombineYawSetInMode()
        {
            // Arrange
            const bool combineYaw = true;
            _mockProgressiveCommand.Setup(x => x.CombineYaw).Returns(combineYaw);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            (_target.Mode & ProgressiveCommandFormatter.Modes.CombineYaw).Should()
                .NotBe((ProgressiveCommandFormatter.Modes)0);
        }

        [TestMethod]
        public void GivenRelativeControl_Load_MagnetiPsiAndMagnetoPsiAccuracySetTo0()
        {
            // Arrange
            _mockProgressiveCommand.Setup(x => x.AbsoluteControlMode).Returns(false);
            _mockProgressiveCommand.Setup(x => x.ControllerHeading).Returns(9.87f);
            _mockProgressiveCommand.Setup(x => x.ControllerHeadingAccuracy).Returns(6.54f);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.MagnetoPsi.Should().Be(0);
            _target.MagnetoPsiAccuracy.Should().Be(0);
        }

        [TestMethod]
        public void GivenAbsoluteControl_Load_MagnetiPsiNormalizedAndConvertedToInt32()
        {
            // Arrange
            _mockProgressiveCommand.Setup(x => x.AbsoluteControlMode).Returns(true);

            TestMagnetoPsi(45, .25f, 123);
            TestMagnetoPsi(90, .5f, 234);
            TestMagnetoPsi(180, 1, 345);
            TestMagnetoPsi(270, -.5f, 456);
            TestMagnetoPsi(360, 0f, 567);
            TestMagnetoPsi(405, .25f, 678);
            TestMagnetoPsi(0, 0f, 789);
            TestMagnetoPsi(-45, -.25f, 890);
            TestMagnetoPsi(-90, -.5f, 012);
            TestMagnetoPsi(-180, 1, 1234);
            TestMagnetoPsi(-270, .5f, 2345);
            TestMagnetoPsi(-360, 0f, 3456);
            TestMagnetoPsi(-405, -.25f, 4567);
        }

        [TestMethod]
        public void GivenAbsoluteControl_Load_MagnetiPsiAccuracyNormalizedAndConvertedToInt32()
        {
            // Arrange
            _mockProgressiveCommand.Setup(x => x.AbsoluteControlMode).Returns(true);

            TestMagnetoPsiAccuracy(90, .25f, 234);
            TestMagnetoPsiAccuracy(180, .5f, 345);
            TestMagnetoPsiAccuracy(270, .75f, 456);
            TestMagnetoPsiAccuracy(360, 1, 567);
            TestMagnetoPsiAccuracy(450, 1.25f, 678);
            TestMagnetoPsiAccuracy(0, 0f, 789);
            TestMagnetoPsiAccuracy(-90, .25f, 012);
            TestMagnetoPsiAccuracy(-180, .5f, 1234);
            TestMagnetoPsiAccuracy(-270, .75f, 2345);
            TestMagnetoPsiAccuracy(-360, 1f, 3456);
            TestMagnetoPsiAccuracy(-450, 1.25f, 4567);
        }

        [TestMethod]
        public void GivenAbsoluteControl_Load_AbsoluteControlSetOnMode()
        {
            // Arrange
            _mockProgressiveCommand.Setup(x => x.AbsoluteControlMode).Returns(true);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            (_target.Mode & ProgressiveCommandFormatter.Modes.AbsoluteControl).Should()
                .NotBe((ProgressiveCommandFormatter.Modes)0);
        }

        private void TestMagnetoPsi(float angle, float normalizedAngle, int asInt32)
        {
            _mockProgressiveCommand.Setup(x => x.ControllerHeading).Returns(angle);
            _mockFloatToInt32Converter.Setup(x => x.Convert(normalizedAngle)).Returns(asInt32);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.MagnetoPsi.Should().Be(asInt32);
        }

        private void TestMagnetoPsiAccuracy(float angle, float normalizedAngle, int asInt32)
        {
            _mockProgressiveCommand.Setup(x => x.ControllerHeadingAccuracy).Returns(angle);
            _mockFloatToInt32Converter.Setup(x => x.Convert(normalizedAngle)).Returns(asInt32);

            // Act
            _target.Load(_mockProgressiveCommand.Object);

            // Assert
            _target.MagnetoPsiAccuracy.Should().Be(asInt32);
        }
    }
}
