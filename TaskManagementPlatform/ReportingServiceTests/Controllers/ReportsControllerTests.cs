using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ReportingService.Controllers;
using ReportingService.Models;
using ReportingService.Services;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReportingService.Tests
{
    [TestClass]
    public class ReportsControllerTests
    {
        private Mock<IReportingService> _svcMock;
        private ReportsController _controller;

        private void SetAuthHeader(string token)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = $"Bearer {token}";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TestInitialize]
        public void Setup()
        {
            _svcMock = new Mock<IReportingService>();
            _controller = new ReportsController(_svcMock.Object);
        }

        [TestMethod]
        public async Task SlaBreaches_ReturnsOkWithTasks()
        {
            // Arrange
            var token = "test-token";
            var expectedTasks = new List<TaskDto>
            {
                new TaskDto { Id = "1", Title = "T1", Status = "Open", AssigneeId = "U1", DueDate = DateTime.UtcNow }
            };
            _svcMock.Setup(s => s.GetSlaBreachesAsync(token)).ReturnsAsync(expectedTasks);
            SetAuthHeader(token);

            // Act
            var result = await _controller.SlaBreaches();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var actualTasks = okResult.Value as List<TaskDto>;
            Assert.IsNotNull(actualTasks);
            Assert.AreEqual(expectedTasks.Count, actualTasks.Count);
            Assert.AreEqual(expectedTasks[0].Id, actualTasks[0].Id);
        }

        [TestMethod]
        public async Task Summary_ReturnsOkWithReport()
        {
            // Arrange
            var token = "test-token";
            var expectedReport = new ReportSummary
            {
                TotalTasks = 10,
                OpenTasks = 5,
                CompletedTasks = 5,
                SlaBreaches = 2,
                TasksByUser = new Dictionary<string, int> { { "U1", 3 } },
                TasksByStatus = new Dictionary<string, int> { { "Open", 5 } }
            };
            _svcMock.Setup(s => s.GetSummaryReportAsync(token)).ReturnsAsync(expectedReport);
            SetAuthHeader(token);

            // Act
            var result = await _controller.Summary();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var actualReport = okResult.Value as ReportSummary;
            Assert.IsNotNull(actualReport);
            Assert.AreEqual(expectedReport.TotalTasks, actualReport.TotalTasks);
        }

        [TestMethod]
        public async Task TasksByUser_ReturnsOkWithTasks()
        {
            // Arrange
            var token = "test-token";
            var userId = "U1";
            var expectedTasks = new List<TaskDto>
            {
                new TaskDto { Id = "2", Title = "T2", Status = "Closed", AssigneeId = userId, DueDate = DateTime.UtcNow }
            };
            _svcMock.Setup(s => s.GetTasksByUserAsync(userId, token)).ReturnsAsync(expectedTasks);
            SetAuthHeader(token);

            // Act
            var result = await _controller.TasksByUser(userId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var actualTasks = okResult.Value as List<TaskDto>;
            Assert.IsNotNull(actualTasks);
            Assert.AreEqual(expectedTasks.Count, actualTasks.Count);
            Assert.AreEqual(expectedTasks[0].Id, actualTasks[0].Id);
        }

        [TestMethod]
        public async Task GetToken_ThrowsException_WhenHeaderMissing()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await _controller.SlaBreaches());
            StringAssert.Contains(ex.Message, "Missing or invalid token");
        }

        [TestMethod]
        public async Task GetToken_ThrowsException_WhenHeaderInvalid()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "InvalidToken";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<Exception>(async () => await _controller.SlaBreaches());
            StringAssert.Contains(ex.Message, "Missing or invalid token");
        }
    }
}
