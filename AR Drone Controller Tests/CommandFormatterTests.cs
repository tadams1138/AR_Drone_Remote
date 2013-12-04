using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class CommandFormatterTests
    {
        private CommandFormatter _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new CommandFormatter();
        }
        
        [TestMethod]
        public void GivenCommandType_CreateCommand_ReturnsFormattedCommand()
        {
            // Arrange
            const string commandTypeA = "commandTypeA";
            const string commandTypeB = "commandTypeB";
            const string commandTypeC = "commandTypeC";

            // Act
            var result1 = _target.CreateCommand(commandTypeA);
            var result2 = _target.CreateCommand(commandTypeB);
            var result3 = _target.CreateCommand(commandTypeC);

            // Assert
            result1.Should().Be("AT*" + commandTypeA + "=1\r");
            result2.Should().Be("AT*" + commandTypeB + "=2\r");
            result3.Should().Be("AT*" + commandTypeC + "=3\r");
        }

        [TestMethod]
        public void GivenCommandTypeAndMessage_CreateCommand_ReturnsFormattedCommand()
        {
            // Arrange
            const string commandTypeA = "commandTypeA";
            const string commandTypeB = "commandTypeB";
            const string commandTypeC = "commandTypeC";
            const string message1 = "messageX";
            const string message2 = "messageY";
            const string message3 = "messageZ";

            // Act
            var result1 = _target.CreateCommand(commandTypeA, message1);
            var result2 = _target.CreateCommand(commandTypeB, message2);
            var result3 = _target.CreateCommand(commandTypeC, message3);

            // Assert
            result1.Should().Be("AT*" + commandTypeA + "=1," + message1 + "\r");
            result2.Should().Be("AT*" + commandTypeB + "=2," + message2 + "\r");
            result3.Should().Be("AT*" + commandTypeC + "=3," + message3 + "\r");
        }
    }
}
