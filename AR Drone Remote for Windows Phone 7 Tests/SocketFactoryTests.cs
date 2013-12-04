using AR_Drone_Controller;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Remote_for_Windows_Phone_7
{
    [TestClass]
    public class SocketFactoryTests
    {
        private SocketFactory _target;

        [TestInitialize]
        public void InitializeTests()
        {
            _target = new SocketFactory();
        }

        [TestMethod]
        public void GetUdpSocket_ReturnsIUdpSocket()
        {
            // Arrange
            const string address = "address test value";
            const int port = 5;

            // Act
            var result = _target.GetUdpSocket(address, port);

            // Assert
            result.Should().BeAssignableTo<IUdpSocket>();
        }

        [TestMethod]
        public void GetTcpSocket_ReturnsITcpSocket()
        {
            // Arrange
            const string address = "address test value";
            const int port = 5;

            // Act
            var result = _target.GetTcpSocket(address, port);

            // Assert
            result.Should().BeAssignableTo<ITcpSocket>();
        }
    }
}
