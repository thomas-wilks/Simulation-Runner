using System.Threading.Tasks;
using MongoDB.Driver;
using Simulate.FostersAndPartners.Shared.Models;

namespace Simulate.FostersAndPartners.Shared.Data
{
    public interface IDbContext
    {
        IMongoCollection<Simulation> Simulations { get; }
        IMongoCollection<Result> Results { get; }
        public Task Initialise();
    }
}