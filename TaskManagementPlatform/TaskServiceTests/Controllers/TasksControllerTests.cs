using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TaskService.Controllers;
using TaskService.Models;
using TaskService.Services;
using System.Diagnostics.CodeAnalysis; // Add this at the top of the file if not already present.

namespace TaskServiceTests.Controllers
{
    [TestClass]
    public class TasksControllerTests
    {
        private Mock<ITaskService> _svc;
        [AllowNull] // Add this attribute to suppress the warning for nullable reference types.
        private TasksController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _svc = new Mock<ITaskService>();
            _controller = new TasksController(_svc.Object);
        }

        [TestMethod]
        public async Task Get_ReturnsOk_WhenTaskExists()
        {
            var dto = new TaskDto { Id = "1" };
            _svc!.Setup(s => s.GetTaskAsync("1")).ReturnsAsync(dto);

            var result = await _controller!.Get("1");

            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(dto, ok.Value);
        }

        [TestMethod]
        public async Task Get_ReturnsNotFound_WhenTaskMissing()
        {
            _svc!.Setup(s => s.GetTaskAsync("2")).ReturnsAsync((TaskDto?)null);

            var result = await _controller!.Get("2");

            Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        }

        [TestMethod]
        public async Task List_ReturnsOkWithTasks()
        {
            var dtos = new List<TaskDto> { new TaskDto { Id = "1" } };
            _svc!.Setup(s => s.ListTasksAsync("Open", "A", null, null)).ReturnsAsync(dtos);

            var result = await _controller!.List("Open", "A", null, null);

            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(dtos, ok.Value);
        }

        [TestMethod]
        public async Task Create_ReturnsCreated_WhenTokenValid()
        {
            var req = new CreateTaskRequest { Title = "T", AssigneeId = "A" };
            var dto = new TaskDto { Id = "1" };
            _svc!.Setup(s => s.CreateTaskAsync(req, "user1")).ReturnsAsync(dto);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer user1_token";
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Create(req);

            var created = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(created);
            Assert.AreEqual(dto, created.Value);
        }

        [TestMethod]
        public async Task Create_ReturnsBadRequest_WhenTokenMissing()
        {
            var req = new CreateTaskRequest { Title = "T", AssigneeId = "A" };
            var httpContext = new DefaultHttpContext();
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Create(req);

            var bad = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            StringAssert.Contains(bad?.Value?.ToString() ?? string.Empty, "token", StringComparison.OrdinalIgnoreCase);
        }

        [TestMethod]
        public async Task Update_ReturnsOk_WhenTokenValid()
        {
            var req = new UpdateTaskRequest { Status = "Closed" };
            _svc!.Setup(s => s.UpdateTaskAsync("1", req, "user1", "user1_token")).Returns(Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer user1_token";
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Update("1", req);

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual("Task updated!", ok.Value);
        }

        [TestMethod]
        public async Task Update_ReturnsBadRequest_WhenTokenInvalid()
        {
            var req = new UpdateTaskRequest { Status = "Closed" };
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "InvalidHeader";
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Update("1", req);

            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            StringAssert.Contains(bad?.Value?.ToString() ?? string.Empty, "token", StringComparison.OrdinalIgnoreCase);
        }

        [TestMethod]
        public async Task Delete_ReturnsOk_WhenTokenValid()
        {
            _svc!.Setup(s => s.DeleteTaskAsync("1", "user1_token")).Returns(Task.CompletedTask);

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer user1_token";
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Delete("1");

            var ok = result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual("Task deleted Successfully!", ok.Value);
        }

        [TestMethod]
        public async Task Delete_ReturnsBadRequest_WhenTokenMissing()
        {
            var httpContext = new DefaultHttpContext();
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Delete("1");

            var bad = result as BadRequestObjectResult;
            Assert.IsNotNull(bad);
            StringAssert.Contains(bad?.Value?.ToString() ?? string.Empty, "token", StringComparison.OrdinalIgnoreCase);
        }

        [TestMethod]
        public async Task Delete_ReturnsForbidden_WhenServiceThrows()
        {
            _svc!.Setup(s => s.DeleteTaskAsync("1", "user1_token")).ThrowsAsync(new Exception("forbidden"));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "Bearer user1_token";
            _controller!.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var result = await _controller.Delete("1");

            var forbidden = result as ObjectResult;
            Assert.IsNotNull(forbidden);
            Assert.AreEqual(403, forbidden.StatusCode);
            Assert.AreEqual("forbidden", forbidden.Value);
        }

        [TestMethod]
        public async Task SlaBreaches_ReturnsOkWithList()
        {
            var dtos = new List<TaskDto> { new TaskDto { Id = "1" } };
            _svc!.Setup(s => s.GetSlaBreachesAsync()).ReturnsAsync(dtos);

            var result = await _controller!.SlaBreaches();

            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(dtos, ok.Value);
        }
    }
}

