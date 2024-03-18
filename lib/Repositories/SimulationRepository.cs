using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Simulate.FostersAndPartners.Shared.Constants;
using Simulate.FostersAndPartners.Shared.Data;
using Simulate.FostersAndPartners.Shared.Helpers;
using Simulate.FostersAndPartners.Shared.Models;

namespace Simulate.FostersAndPartners.Shared.Repositories
{
    public class SimulationRepository : ISimulationRepository
    {
        private readonly IDbContext _dbContext;
        public SimulationRepository(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Simulation> CreateQueuedSimulation(int priority, CancellationToken cancellationToken)
        {
            var simulation = new Simulation
            {
                RequesterId = "dummyUser",
                Priority = priority,
                Status = SimulationStatusConstants.Queued,
                QueuedAt = DateTime.UtcNow
            };
            await _dbContext.Simulations.InsertOneAsync(simulation, cancellationToken: cancellationToken);
            return simulation;
        }

        public async Task<Result> GetResult(string id, CancellationToken cancellationToken)
        {
            if (!DatabaseHelpers.IsValidObjectId(id))
            {
                return null;
            }
            return await _dbContext.Results.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Simulation> GetSimulation(string id, CancellationToken cancellationToken)
        {
            if (!DatabaseHelpers.IsValidObjectId(id))
            {
                return null;
            }
            return await _dbContext.Simulations.Find(s => s.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InsertResult(Result result, CancellationToken cancellationToken)
        {
            await _dbContext.Results.InsertOneAsync(result, cancellationToken: cancellationToken);
        }

        public async Task<Simulation> UpdateSimulationStatus(string id, string status, CancellationToken cancellationToken)
        {
            var update = Builders<Simulation>.Update
                .Set(s => s.Status, status);

            switch (status)
            {
                case SimulationStatusConstants.Started:
                    update = update.Set(s => s.StartedAt, DateTime.UtcNow);
                    break;
                case SimulationStatusConstants.Completed:
                    update = update.Set(s => s.CompletedAt, DateTime.UtcNow);
                    break;
            }

            return await _dbContext.Simulations.FindOneAndUpdateAsync<Simulation>(
                s => s.Id == id,
                update,
                new FindOneAndUpdateOptions<Simulation>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken
            );
        }
    }
}