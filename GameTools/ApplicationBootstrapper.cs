using LVK.Bootstrapping;
using LVK.Hosting;
using LVK.Pushover;

using Radzen;

namespace GameTools;

public class ApplicationBootstrapper : IModuleBootstrapper
{
    public void Bootstrap(IHostApplicationBuilder builder)
    {
        builder.AddStandardConfigurationSources<Program>();

        builder.Bootstrap(new LVK.Events.ModuleBootstrapper());
        builder.Bootstrap(new Database.ModuleBootstrapper());
        builder.Bootstrap(new Workers.ModuleBootstrapper());

        builder.Services.AddPushoverClient(options =>
        {
            options.UseApiToken(builder.Configuration["Pushover:ApiToken"] ?? throw new InvalidOperationException("No Pushover API token configured"));
        });

        // Add services to the container.
        builder.Services.AddRazorComponents()
           .AddInteractiveServerComponents();

        builder.Services.AddRadzenComponents();

        builder.Services.AddTransient<IHostInitializer<WebApplication>, ApplicationInitializer>();
    }
}