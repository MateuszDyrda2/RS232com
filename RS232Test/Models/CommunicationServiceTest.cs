using Moq;
using RS232DTE.Models;
using RS232DTE.Models.Enums;

namespace RS232Test.Models
{
    public class CommunicationServiceTest
    {
        public CommunicationServiceTest()
        { }

        #region RecreatePort
        [Fact]
        public void RecreatePort_NullPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();

            // Act => Assert
            Assert.Throws<ArgumentNullException>(() => service.RecreatePort(null));
        }

        [Theory]
        [InlineData("port2", 0, 0)]
        [InlineData("com", 0, 0)]
        [InlineData("COM1", 0, 0)]
        [InlineData("COM2", 150, 0)]
        [InlineData("COM3", 300, 0)]
        [InlineData("COM4", 14400, 3)]
        [InlineData("COM15", 115200, 1)]
        public void RecreatePort_InvalidPortParameters_ThrowsArgumentException(string portName, int baudRate, int bitCount)
        {
            // Arrange
            var service = GetService();
            var port = GetPortForRecreatePort(portName, baudRate, bitCount, "t");

            // Act => Assert
            Assert.Throws<ArgumentException>(() => service.RecreatePort(port.Object));
        }

        [Theory]
        [InlineData("COM1", null)]
        [InlineData(null, "s")]
        public void RecreatePort_NullPortParameters_ThrowsArgumentNullException(string portName, string terminator)
        {
            // Arrange
            var service = GetService();
            var port = GetPortForRecreatePort(portName, 300, 8, terminator);

            // Act => Assert
            Assert.Throws<ArgumentNullException>(() => service.RecreatePort(port.Object));
        }

        [Fact]
        public void RecreatePort_CorrectData_RecreatesPort()
        {
            // Arrange
            var service = GetService();
            var portName = "COM1";
            var baudRate = 150;
            var bitCount = 7;
            var terminator = "\\n";
            var port = GetPortForRecreatePort(portName, baudRate, bitCount, terminator);

            // Act 
            service.RecreatePort(port.Object);

            // Assert
            Assert.Equal(portName, service.Port.PortName);
            Assert.Equal(baudRate, service.Port.BaudRate);
            Assert.Equal(bitCount, service.Port.BitCount);
            Assert.Equal(terminator, service.Port.Terminator);
        }

        [Fact]
        public void RecreatePort_PortAlreadySet_DisposesOfPrevious()
        {
            // Arrange
            var service = GetService();
            var port = GetPortForRecreatePort("COM1", 150, 7, "\\n");
            var port2 = GetPortForRecreatePort("COM2", 150, 7, "\\n");
            service.RecreatePort(port.Object);

            // Act
            service.RecreatePort(port2.Object);

            // Assert
            port.Verify(x => x.Dispose());
        }

        #endregion

        #region Connect

        [Fact]
        public void Connect_NullPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();

            // Act => Assert
            Assert.Throws<ArgumentNullException>(() => service.Connect());
        }

        [Fact]
        public void Connect_SuccessfulyConnected_IsConnectedTrue()
        {
            // Arrange
            var service = GetServiceWithPort();

            // Act
            service.Connect();

            // Assert
            Assert.True(service.IsConnected);
        }

        [Fact]
        public void Connect_PortIsNotOpened_Throws()
        {
            // Arrange
            var service = GetService();
            var port = GetPortMock();
            port.Setup(x => x.Open()).Throws<UnauthorizedAccessException>();
            service.RecreatePort(port.Object);

            // Act => Assert
            Assert.Throws<UnauthorizedAccessException>(() => service.Connect());
            Assert.False(service.IsConnected);
        }
        #endregion

        #region CloseConnection

        [Fact]
        public void CloseConnection_NullPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();

            // Act => Assert
            Assert.Throws<ArgumentNullException>(() => service.CloseConnection());
        }

        [Fact]
        public void CloseConnection_OpenPort_IsConnectedFalse()
        {
            // Arrange
            var service = GetServiceWithPort();

            // Act
            service.CloseConnection();

            // Assert
            Assert.False(service.IsConnected);
        }
        #endregion

        #region WriteMessage

        [Fact]
        public async Task WriteMessage_NullPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();

            // Act => Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                async () => await service.WriteMessage(""));

        }
        [Fact]
        public async Task WriteMessage_PortNotOpened_ThrowsArgumentException()
        {
            // Arrange
            var service = GetServiceWithPort();

            // Act => Assert
            await Assert.ThrowsAsync<ArgumentException>(
                async () => await service.WriteMessage(""));
        }
        [Fact]
        public async Task WriteMessage_CorrectMessage_WritesMessage()
        {
            // Arrange
            var service = GetService();
            var port = GetPortMock();
            port.Setup(x => x.IsOpen).Returns(true);
            service.RecreatePort(port.Object);
            var message = "hello";

            // Act
            await service.WriteMessage(message);

            // Assert
            port.Verify(x => x.WriteLineAsync(message));
        }
        #endregion

        #region Ping

        [Fact]
        public async Task Ping_NullPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();

            // Act => Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await service.Ping());
        }

        [Fact]
        public async Task Ping_ClosedPort_ThrowsException()
        {
            // Arrange
            var service = GetServiceWithPort();

            // Act => Assert
            await Assert.ThrowsAsync<Exception>(async () => await service.Ping());
        }
        [Fact]
        public async Task Ping_OpenPort_ThrowsArgumentNullException()
        {
            // Arrange
            var service = GetService();
            var port = GetPortMock();
            port.Setup(x => x.IsOpen).Returns(true);
            service.RecreatePort(port.Object);

            // Act
            await service.Ping();

            // Assert
            port.Verify(x => x.WriteLineAsync(It.IsAny<string>()));
        }
        #endregion

        #region GetAllSerialDevices

        #endregion

        private CommunicationService GetService() => new CommunicationService();

        private CommunicationService GetServiceWithPort()
        {
            var port = GetPortMock();
            var service = GetService();

            service.RecreatePort(port.Object);
            return service;
        }

        private Mock<IPort> GetPortMock()
        {
            var port = new Mock<IPort>();

            port.Setup(x => x.PortName).Returns("COM1");
            port.Setup(x => x.BaudRate).Returns(300);
            port.Setup(x => x.BitCount).Returns(8);
            port.Setup(x => x.Terminator).Returns("\\n");
            port.Setup(x => x.StopBits).Returns(It.IsAny<StopBitsType>());
            port.Setup(x => x.FlowControl).Returns(It.IsAny<FlowControlTypes>());
            port.Setup(x => x.Parity).Returns(It.IsAny<ParityTypes>());
            port.Setup(x => x.ReadTimeout).Returns(500);
            port.Setup(x => x.WriteTimeout).Returns(500);

            return port;
        }


        private Mock<IPort> GetPortForRecreatePort(
            string portName, int baudRate, int bitCount, string terminator)
        {
            var port = new Mock<IPort>();
            port.Setup(x => x.PortName).Returns(portName);
            port.Setup(x => x.BaudRate).Returns(baudRate);
            port.Setup(x => x.BitCount).Returns(bitCount);
            port.Setup(x => x.Terminator).Returns(terminator);

            return port;
        }

    }
}
