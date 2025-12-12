using LVK.Bootstrapping;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameTools.Database;

public class ModuleBootstrapper : IModuleBootstrapper
{
    public void Bootstrap(IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<GameToolsDbContext>(options =>
        {
            string connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("No connection string found for 'Default' connection");

            options.UseNpgsql(connectionString, configure => configure.SetPostgresVersion(18, 0));
        });

        builder.Services.AddTransient<IModuleInitializer, ModuleInitializer>();
    }
}