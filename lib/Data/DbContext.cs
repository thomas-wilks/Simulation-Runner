using System.Threading.Tasks;
using MongoDB.Driver;
using Simulate.FostersAndPartners.Shared.Models;
using Microsoft.Extensions.Options;

namespace Simulate.FostersAndPartners.Shared.Data
{
    public class DbContext : IDbContext
    {
        public IMongoCollection<Simulation> Simulations { get; }
        public IMongoCollection<Result> Results { get; }

        public DbContext(IOptions<Storage> options)
        {
            var client = new MongoClient(options.Value.MongoConnectionString);
            var database = client.GetDatabase(options.Value.MongoDatabaseName);
            Simulations = database.GetCollection<Simulation>("SimulationService:Simulations");
            Results = database.GetCollection<Result>("SimulationService:Results");
        }

        public async Task Initialise()
        {
            CreateIndexModel<Simulation>[] simulationIndexes = {
                new CreateIndexModel<Simulation>(
                    Builders<Simulation>.IndexKeys.Ascending(x => x.Priority)
                ),
                new CreateIndexModel<Simulation>(
                    Builders<Simulation>.IndexKeys.Ascending(x => x.Status)
                ),
                new CreateIndexModel<Simulation>(
                    Builders<Simulation>.IndexKeys.Ascending(x => x.QueuedAt)
                ),
                new CreateIndexModel<Simulation>(
                    Builders<Simulation>.IndexKeys.Ascending(x => x.CompletedAt)
                ),
                new CreateIndexModel<Simulation>(
                    Builders<Simulation>.IndexKeys.Ascending(x => x.RequesterId)
                )
            };
            await Simulations.Indexes.CreateManyAsync(simulationIndexes);
        }
    }
}