using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using EShop.Services;

namespace EShop.Tests
{
    [TestFixture]
    public class EmailServiceTests
    {
        private EmailService _emailService;
        private Mock<IConfiguration> _configMock;

        [SetUp]
        public void Setup()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["Smtp:Host"]).Returns("smtp.test.com");
            _configMock.Setup(c => c["Smtp:Port"]).Returns("587");
            _configMock.Setup(c => c["Smtp:Username"]).Returns("test@test.com");
            _configMock.Setup(c => c["Smtp:AppKey"]).Returns("testkey");
            _emailService = new EmailService(_configMock.Object);
        }

        [Test]
        public void EmailService_Constructor_InitializesCorrectly()
        {
            Assert.That(_emailService, Is.Not.Null);
        }

        [Test]
        public void SendEmailAsync_ThrowsException_WhenSmtpFails()
        {
            // This will fail to connect to the test SMTP server
            Assert.ThrowsAsync<System.Net.Sockets.SocketException>(async () => 
                await _emailService.SendEmailAsync("test@test.com", "Test Subject", "Test Body"));
        }
    }
}