using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class CommandQueueTests
    {
        private CommandQueue _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new CommandQueue();
        }

        [TestMethod]
        public void GivenMessageInTheQueue_Flush_ReturnsMessage()
        {
            // Arrange
            const string testMessage = "testMessage";
            _target.Enqueue(testMessage);

            // Act 
            string result = _target.Flush();

            // Assert
            result.Should().Be(testMessage);
        }

        [TestMethod]
        public void GivenNoMessageInTheQueue_Flush_ReturnsNull()
        {
            // Arrange

            // Act 
            string result = _target.Flush(); // should be near instantaneous from last call

            // Assert
            result.Should().BeNull();
        }

        [TestMethod]
        public void GivenMultipleMessages_Flush_ReturnsConcatenatedResult()
        {
            // Arrange
            const string testMessage1 = "testMessage1";
            _target.Enqueue(testMessage1);
            const string testMessage2 = "testMessage2";
            _target.Enqueue(testMessage2);
            const string testMessage3 = "testMessage3";
            _target.Enqueue(testMessage3);

            // Act 
            string result = _target.Flush();

            // Assert
            result.Should().Be(testMessage1 + testMessage2 + testMessage3);
        }

        [TestMethod]
        public void GivenEnoughCommandsToExceedMaxMessageLength_Flush_ReturnsConcatenatedResultsNotExceedingMaxMessageLength()
        {
            // Arrange
            var testMessage1 = new string('a', CommandQueue.MaxMessageLength / 4);
            _target.Enqueue(testMessage1);
            var testMessage2 = new string('b', CommandQueue.MaxMessageLength / 4);
            _target.Enqueue(testMessage2);
            var testMessage3 = new string('c', CommandQueue.MaxMessageLength / 4);
            _target.Enqueue(testMessage3);
            var testMessage4 = new string('d', CommandQueue.MaxMessageLength / 2);
            _target.Enqueue(testMessage4);
            var testMessage5 = new string('e', CommandQueue.MaxMessageLength / 4);
            _target.Enqueue(testMessage5);
            var testMessage6 = new string('f', CommandQueue.MaxMessageLength);
            _target.Enqueue(testMessage6);
            var testMessage7 = new string('g', CommandQueue.MaxMessageLength /4);
            _target.Enqueue(testMessage7);

            // Act 
            string result1 = _target.Flush();
            string result2 = _target.Flush();
            string result3 = _target.Flush();
            string result4 = _target.Flush();

            // Assert
            result1.Should().Be(testMessage1 + testMessage2 + testMessage3);
            result2.Should().Be(testMessage4 + testMessage5);
            result3.Should().Be(testMessage6);
            result4.Should().Be(testMessage7);
        }

        [TestMethod]
        [ExpectedException(typeof(CommandQueue.CommandTooLongException))]
        public void GivenCommandThatExceedsMaxMessageLength_Enqueu_ThrowsException()
        {
            // Arrange
            string tooLongMessage = new string('X', CommandQueue.MaxMessageLength+1);

            // Act
            _target.Enqueue(tooLongMessage);

            // Assert
            // Should have thrown exception
        }

        [TestClass]
        public class CommandTooLongExceptionTests
        {
            [TestMethod]
            public void GivenCommandInConstrctor_CommandProperty_ReturnsCommand()
            {
                // Arrange
                const string testCommand = "testCommand";
                
                // Act
                var result = new CommandQueue.CommandTooLongException(testCommand);

                // Assert
                result.Command.Should().Be(testCommand);
            }
        }
    }
}
