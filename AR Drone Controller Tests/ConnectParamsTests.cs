using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AR_Drone_Controller
{
    [TestClass]
    public class ConnectParamsTests
    {
        [TestMethod]
        public void Contructor_LoadsDefaultValues()
        {
            // Arrange

            // Act
            var result = new ConnectParams();

            // Assert
            result.CommandPort.Should().Be(ConnectParams.DefaultCommandPort);
            result.ControlPort.Should().Be(ConnectParams.DefaultControlPort);
            result.VideoPort.Should().Be(ConnectParams.DefaultVideoPort);
            result.NavDataPort.Should().Be(ConnectParams.DefaultNavDataPort);
            result.NetworkAddress.Should().Be(ConnectParams.DefaultNetworkAddress);
        }
    }
}