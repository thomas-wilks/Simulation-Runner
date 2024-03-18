using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Simulate.FostersAndPartners.Service.Models;
using Simulate.FostersAndPartners.Shared.Constants;
using Simulate.FostersAndPartners.Shared.Helpers;
using Simulate.FostersAndPartners.Shared.Providers;
using Simulate.FostersAndPartners.Shared.Repositories;

namespace Simulate.FostersAndPartners.Service.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class SimulateController : ControllerBase
    {
        private readonly ISimulationRepository _simRepo;
        private readonly IMessagePublisher _messagePublisher;

        public SimulateController(ISimulationRepository simulationRepository, IMessagePublisher messagePublisher)
        {
            _simRepo = simulationRepository;
            _messagePublisher = messagePublisher;
        }

        [HttpPost]
        public async Task<IActionResult> Post(QueueSimulationRequestModel request, CancellationToken cancellationToken)
        {
            // Check to ensure that priority is within the allowed range
            if (request.Priority < 1 || request.Priority > 5)
            {
                return BadRequest();
            }

            var simulation = await _simRepo.CreateQueuedSimulation(request.Priority, cancellationToken);

            _messagePublisher.PublishMessage(simulation.Id);

            return Ok(new SimulationAcceptedResponseModel
            {
                Id = simulation.Id
            });
        }

        [HttpGet("{id}/status")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            if (!DatabaseHelpers.IsValidObjectId(id))
            {
                return BadRequest();
            }
            var simulation = await _simRepo.GetSimulation(id, cancellationToken);
            if (simulation is null)
            {
                return NotFound();
            }
            return Ok(new SimulationStatusResponseModel
            {
                Status = simulation.Status,
                QueuedAt = simulation.QueuedAt,
                StartedAt = simulation.StartedAt,
                CompletedAt = simulation.CompletedAt
            });
        }

        [HttpGet("{id}/result")]
        public async Task<IActionResult> GetResult(string id, CancellationToken cancellationToken)
        {
            if (!DatabaseHelpers.IsValidObjectId(id))
            {
                return BadRequest();
            }
            var simulation = await _simRepo.GetSimulation(id, cancellationToken);
            if (simulation is null || simulation.Status != SimulationStatusConstants.Completed)
            {
                return BadRequest();
            }

            var result = await _simRepo.GetResult(id, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(new SimulationResultsResponseModel
            {
                Result = result.Data
            });
        }
    }
}