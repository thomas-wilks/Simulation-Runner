using System.Threading;
using System.Threading.Tasks;
using Simulate.FostersAndPartners.Shared.Models;

namespace Simulate.FostersAndPartners.Shared.Repositories
{
    public interface ISimulationRepository
    {
        Task<Simulation> CreateQueuedSimulation(int priority, CancellationToken cancellationToken);
        Task<Simulation> GetSimulation(string id, CancellationToken cancellationToken);
        Task<Simulation> UpdateSimulationStatus(string id, string status, CancellationToken cancellationToken);
        Task<Result> GetResult(string id, CancellationToken cancellationToken);
        Task InsertResult(Result result, CancellationToken cancellationToken);
    }
}