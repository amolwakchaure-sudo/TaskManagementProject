using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using UserService.Controllers;
using UserService.Models;
using UserService.Repositories;

namespace UserService.Tests
{
    [TestClass]
    public class AuthControllerTests
    {
        private Mock<IUserRepository> _repoMock;
        private Mock<ILogger<AuthController>> _loggerMock;
        private AuthController _controller;

        [TestInitialize]
        public void Setup()
        {
            _repoMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_repoMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_IfUsernameOrPasswordMissing()
        {
            // Arrange
            var model1 = new LoginModel { Username = null, Password = "pass" };
            var model2 = new LoginModel { Username = "user", Password = null };
            var model3 = new LoginModel { Username = "", Password = "pass" };
            var model4 = new LoginModel { Username = "user", Password = "" };

            // Act
            var result1 = await _controller.Login(model1);
            var result2 = await _controller.Login(model2);
            var result3 = await _controller.Login(model3);
            var result4 = await _controller.Login(model4);

            // Assert
            Assert.IsInstanceOfType(result1, typeof(BadRequestObjectResult));
            Assert.IsInstanceOfType(result2, typeof(BadRequestObjectResult));
            Assert.IsInstanceOfType(result3, typeof(BadRequestObjectResult));
            Assert.IsInstanceOfType(result4, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Login_ReturnsUnauthorized_IfUserNotFound()
        {
            // Arrange
            var model = new LoginModel { Username = "user", Password = "pass" };
            _repoMock.Setup(r => r.AuthenticateAsync(model.Username, model.Password)).ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual("Invalid username or password", unauthorized.Value);
        }

        [TestMethod]
        public async Task Login_ReturnsOk_WithToken_IfUserFound()
        {
            // Arrange
            var model = new LoginModel { Username = "user", Password = "pass" };
            var user = new User { Id = "abc123", Username = "user", Role = "admin" };
            _repoMock.Setup(r => r.AuthenticateAsync(model.Username, model.Password)).ReturnsAsync(user);

            // Act
            var result = await _controller.Login(model);

            // Assert
            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            dynamic value = ok.Value;
            Assert.IsNotNull(value);
            Assert.AreEqual("abc123_admin", value.Token);
        }

        [TestMethod]
        public async Task Login_Returns500_AndLogsError_OnException()
        {
            // Arrange
            var model = new LoginModel { Username = "user", Password = "pass" };
            _repoMock.Setup(r => r.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("db error"));

            // Act
            var result = await _controller.Login(model);

            // Assert
            var status = result as ObjectResult;
            Assert.IsNotNull(status);
            Assert.AreEqual(500, status.StatusCode);
            Assert.AreEqual("Internal server error", status.Value);
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
