using LVK.Bootstrapping;

using Radzen;

namespace GameTools;

public class ApplicationBootstrapper : IModuleBootstrapper
{
    public void Bootstrap(IHostApplicationBuilder builder)
    {
        builder.Bootstrap(new GameTools.Database.ModuleBootstrapper());

        // Add services to the container.
        builder.Services.AddRazorComponents()
           .AddInteractiveServerComponents();

        builder.Services.AddRadzenComponents();

        builder.Services.AddTransient<IHostInitializer<WebApplication>, ApplicationInitializer>();
    }
}