using Moq;
using Microsoft.AspNetCore.Mvc;
using Simulate.FostersAndPartners.Service.Controllers;
using Simulate.FostersAndPartners.Service.Models;
using Simulate.FostersAndPartners.Shared.Models;
using Simulate.FostersAndPartners.Shared.Providers;
using Simulate.FostersAndPartners.Shared.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulate.FostersAndPartners.Test
{
    public class ApiUnitTest
    {
        private readonly Mock<IMessagePublisher> _mockPublisher;

        public ApiUnitTest()
        {
            _mockPublisher = new Mock<IMessagePublisher>();
        }

        [Fact]
        public async Task Post_ReturnsAccepted()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var request = new QueueSimulationRequestModel
            {
                Priority = 1
            };
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.CreateQueuedSimulation(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1
                });

            var result = await controller.Post(request, cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<SimulationAcceptedResponseModel>(okResult.Value);
            Assert.Equal("5f4e4e3e4e3e4e3e4e3e4e3e", model.Id);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var request = new QueueSimulationRequestModel
            {
                Priority = 10
            };
            var cancellationToken = new CancellationToken();

            var result = await controller.Post(request, cancellationToken);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsOk()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1,
                    Status = "Completed",
                    QueuedAt = DateTime.Now,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                });

            var result = await controller.Get("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<SimulationStatusResponseModel>(okResult.Value);
            Assert.Equal("Completed", model.Status);
        }

        [Fact]
        public async Task GetResult_ReturnsOk()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1,
                    Status = "Completed",
                    QueuedAt = DateTime.Now,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                });

            mockRepo.Setup(repo => repo.GetResult(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Result
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Data = "DummyData"
                });

            var result = await controller.GetResult("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var model = Assert.IsType<SimulationResultsResponseModel>(okResult.Value);
            Assert.Equal("DummyData", model.Result);
        }

        [Fact]
        public async Task GetResult_ReturnsNotFound()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1,
                    Status = "Completed",
                    QueuedAt = DateTime.Now,
                    StartedAt = DateTime.Now,
                    CompletedAt = DateTime.Now
                });

            mockRepo.Setup(repo => repo.GetResult(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Result)null);

            var result = await controller.GetResult("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Simulation)null);

            var result = await controller.Get("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetResult_ReturnsBadRequestOnQueuedState()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1,
                    Status = "Queued",
                    QueuedAt = DateTime.Now
                });

            var result = await controller.GetResult("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetResult_ReturnsBadRequestOnRunningState()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Simulation
                {
                    Id = "5f4e4e3e4e3e4e3e4e3e4e3e",
                    Priority = 1,
                    Status = "Started",
                    QueuedAt = DateTime.Now
                });

            var result = await controller.GetResult("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task GetResult_ReturnsBadRequestOnNullSimulation()
        {
            var mockRepo = new Mock<ISimulationRepository>();
            var controller = new SimulateController(mockRepo.Object, _mockPublisher.Object);
            var cancellationToken = new CancellationToken();

            mockRepo.Setup(repo => repo.GetSimulation(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Simulation)null);

            var result = await controller.GetResult("5f4e4e3e4e3e4e3e4e3e4e3e", cancellationToken);

            Assert.IsType<BadRequestResult>(result);
        }
    }
}