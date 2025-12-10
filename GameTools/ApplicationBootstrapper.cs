using LVK.Bootstrapping;

namespace GameTools;

public class ApplicationBootstrapper : IModuleBootstrapper
{
    public void Bootstrap(IHostApplicationBuilder builder)
    {
        // Add services to the container.
        builder.Services.AddRazorComponents()
           .AddInteractiveServerComponents();

        builder.Services.AddTransient<IHostInitializer<WebApplication>, ApplicationInitializer>();
    }
}