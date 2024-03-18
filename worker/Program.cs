using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Simulate.FostersAndPartners.Shared.Data;
using Simulate.FostersAndPartners.Shared.Repositories;
using Simulate.FostersAndPartners.Worker;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddSingleton<IDbContext, DbContext>();
        builder.Services.AddSingleton<ISimulationRepository, SimulationRepository>();
        builder.Services.Configure<Storage>(builder.Configuration.GetSection("Storage"));

        var host = builder.Build();

        var requiredServices = host.Services;
        await requiredServices.GetRequiredService<IDbContext>().Initialise();
        host.Run();
    }
}