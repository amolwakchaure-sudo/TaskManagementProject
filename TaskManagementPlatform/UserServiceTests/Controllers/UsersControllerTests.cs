using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UserService.Controllers;
using UserService.Models;
using UserService.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace UserService.Tests
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<IUserService> _svcMock;
        private Mock<ILogger<UsersController>> _loggerMock;
        private UsersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _svcMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _controller = new UsersController(_svcMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task Get_ReturnsUser_WhenFound()
        {
            var user = new UserDto { Id = "1", Username = "test", Role = "Admin", CreatedAt = DateTime.UtcNow };
            _svcMock.Setup(s => s.GetUserAsync("1")).ReturnsAsync(user);

            var result = await _controller.Get("1");

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = result.Result as OkObjectResult;
            Assert.AreEqual(user, ok.Value);
        }

        [TestMethod]
        public async Task Get_ReturnsNotFound_WhenUserMissing()
        {
            _svcMock.Setup(s => s.GetUserAsync("2")).ReturnsAsync((UserDto)null);

            var result = await _controller.Get("2");

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task List_ReturnsAllUsers()
        {
            var users = new List<UserDto> { new UserDto { Id = "1", Username = "a", Role = "Admin", CreatedAt = DateTime.UtcNow } };
            _svcMock.Setup(s => s.ListUsersAsync()).ReturnsAsync(users);

            var result = await _controller.List();

            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            var ok = result.Result as OkObjectResult;
            Assert.AreEqual(users, ok.Value);
        }

        [TestMethod]
        public async Task Create_ReturnsCreatedUser()
        {
            var req = new CreateUserRequest { Username = "b", Password = "pw", Role = "User" };
            var user = new UserDto { Id = "2", Username = "b", Role = "User", CreatedAt = DateTime.UtcNow };
            _svcMock.Setup(s => s.CreateUserAsync(req)).ReturnsAsync(user);

            var result = await _controller.Create(req);

            Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
            var created = result.Result as CreatedAtActionResult;
            Assert.AreEqual(user, created.Value);
            Assert.AreEqual("Get", created.ActionName);
            Assert.AreEqual(user.Id, created.RouteValues["id"]);
        }

        [TestMethod]
        public async Task Update_ReturnsNoContent()
        {
            var dto = new UserDto { Id = "3", Username = "c", Role = "User", CreatedAt = DateTime.UtcNow };
            _svcMock.Setup(s => s.UpdateUserAsync("3", dto)).Returns(Task.CompletedTask);

            var result = await _controller.Update("3", dto);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }

        [TestMethod]
        public async Task Delete_Returns401_WhenNoToken()
        {
            SetAuthHeader(null);

            var result = await _controller.Delete("1");

            var obj = result as ObjectResult;
            Assert.AreEqual(401, obj.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Returns401_WhenWrongFormat()
        {
            SetAuthHeader("Basic xyz");

            var result = await _controller.Delete("1");

            var obj = result as ObjectResult;
            Assert.AreEqual(401, obj.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Returns403_WhenNotAdmin()
        {
            SetAuthHeader("Bearer token_User");

            var result = await _controller.Delete("1");

            var obj = result as ObjectResult;
            Assert.AreEqual(403, obj.StatusCode);
        }

        [TestMethod]
        public async Task Delete_Returns404_WhenUserNotFound()
        {
            SetAuthHeader("Bearer token_Admin");
            _svcMock.Setup(s => s.GetUserAsync("1")).ReturnsAsync((UserDto)null);

            var result = await _controller.Delete("1");

            var notFound = result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);
            Assert.AreEqual("User not found", notFound.Value);
        }

        [TestMethod]
        public async Task Delete_ReturnsOk_WhenAdminAndUserExists()
        {
            SetAuthHeader("Bearer token_Admin");
            var user = new UserDto { Id = "1", Username = "d", Role = "User", CreatedAt = DateTime.UtcNow };
            _svcMock.Setup(s => s.GetUserAsync("1")).ReturnsAsync(user);
            _svcMock.Setup(s => s.DeleteUserAsync("1")).Returns(Task.CompletedTask);

            var result = await _controller.Delete("1");

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual("User Deleted Successfully!", ok.Value);
        }

        private void SetAuthHeader(string value)
        {
            var httpContext = new DefaultHttpContext();
            if (value != null)
                httpContext.Request.Headers["Authorization"] = value;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }
    }
}
