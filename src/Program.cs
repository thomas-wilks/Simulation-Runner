using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Simulate.FostersAndPartners.Shared.Data;
using Simulate.FostersAndPartners.Shared.Providers;
using Simulate.FostersAndPartners.Shared.Repositories;

namespace Simulate.FostersAndPartners.Service
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<IDbContext, DbContext>();
            builder.Services.AddSingleton<IMessagePublisher, MessagePublisher>();
            builder.Services.AddSingleton<ISimulationRepository, SimulationRepository>();
            builder.Services.Configure<Storage>(builder.Configuration.GetSection("Storage"));
            builder.Services.AddControllers();

            var app = builder.Build();

            var requiredServices = app.Services;
            await requiredServices.GetRequiredService<IDbContext>().Initialise();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.MapControllers();

            app.Run();
        }
    }
}