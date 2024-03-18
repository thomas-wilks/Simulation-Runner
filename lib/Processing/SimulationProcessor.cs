using System;
using System.Threading;
using System.Threading.Tasks;

namespace Simulate.FostersAndPartners.Shared.Processing
{
    public static class SimulationProcessor
    {
        public static async Task Simulate(CancellationToken cancellationToken)
        {
            var random = new Random();
            int delay = random.Next(15000, 30000); // Random delay between 15 and 30 seconds
            await Task.Delay(delay, cancellationToken);
        }
    }
}