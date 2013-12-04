using System.Threading;
using AR_Drone_Controller.NavData;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AR_Drone_Controller
{
    [TestClass]
    public class WorkerFactoryTests
    {
        private WorkerFactory _target;
        private Mock<ISocketFactory> _mockSocketFactory;
        private ConnectParams _connectParams;

        [TestInitialize]
        public void InitializeTests()
        {
            _mockSocketFactory = new Mock<ISocketFactory>();
            _connectParams = new ConnectParams();
            _target = new WorkerFactory
                {
                    ConnectParams = _connectParams,
                    SocketFactory = _mockSocketFactory.Object,
                };
        }

        [TestMethod]
        public void CreateCommandWorker_CreatesUdpSocketAndAssignsToWorker()
        {
            // Arrange
            IUdpSocket udpSocket = new Mock<IUdpSocket>().Object;
            _mockSocketFactory.Setup(x => x.GetUdpSocket(_connectParams.NetworkAddress, _connectParams.CommandPort))
                              .Returns(udpSocket);

            // Act
            var result = _target.CreateCommandWorker();

            // Assert
            result.Should().BeOfType<CommandWorker>();
            result.Socket.Should().Be(udpSocket);

            result.CommandFormatter.Should().BeOfType<CommandFormatter>();
            result.CommandQueue.Should().BeOfType<CommandQueue>();
            result.FloatToInt32Converter.Should().BeOfType<FloatToInt32Converter>();
            result.ProgressiveCommandFormatter.Should().BeOfType<ProgressiveCommandFormatter>();

            VerifyValidSessionAppAndProfileIds(result);
        }
       
        [TestMethod]
        public void CreateVideoWorker_CreatesTcpSocketAndAssignsToWorker()
        {
            // Arrange
            ITcpSocket tcpSocket = new Mock<ITcpSocket>().Object;
            _mockSocketFactory.Setup(x => x.GetTcpSocket(_connectParams.NetworkAddress, _connectParams.VideoPort))
                              .Returns(tcpSocket);

            // Act
            var result = _target.CreateVideoWorker();

            // Assert
            result.Should().BeOfType<VideoWorker>();
            result.Socket.Should().Be(tcpSocket);
        }

        [TestMethod]
        public void CreateNavDataWorker_CreatesUdpSocketAndAssignsToWorker()
        {
            // Arrange
            IUdpSocket udpSocket = new Mock<IUdpSocket>().Object;
            _mockSocketFactory.Setup(x => x.GetUdpSocket(_connectParams.NetworkAddress, _connectParams.NavDataPort))
                              .Returns(udpSocket);

            // Act
            var result = _target.CreateNavDataWorker();

            // Assert
            result.Should().BeOfType<NavDataWorker>();
            result.Socket.Should().Be(udpSocket);
            result.NavDataFactory.Should().BeOfType<NavDataFactory>();
            result.TimerFactory.Should().BeOfType<TimerFactory>();
            result.TimerFactory.TimerCallback.Should().Be((TimerCallback)result.CheckTimeout);
            result.TimerFactory.Period.Should().Be(NavDataWorker.Timeout);
        }

        [TestMethod]
        public void CreateControlWorker_CreatesTcpSocketAndAssignsToWorker()
        {
            // Arrange
            ITcpSocket tcpSocket = new Mock<ITcpSocket>().Object;
            _mockSocketFactory.Setup(x => x.GetTcpSocket(_connectParams.NetworkAddress, _connectParams.ControlPort))
                              .Returns(tcpSocket);

            // Act
            var result = _target.CreateControlWorker();

            // Assert
            result.Should().BeOfType<ControlWorker>();
            result.Socket.Should().Be(tcpSocket);
        }
    
        private static void VerifyValidSessionAppAndProfileIds(CommandWorker result)
        {
            result.SessionId.Should().HaveLength(8);
            result.ProfileId.Should().HaveLength(8);
            result.ApplicationId.Should().HaveLength(8);
        }
    }
}
