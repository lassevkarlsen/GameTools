using GameTools.Workers.TimerNotifications;

using LVK.Bootstrapping;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GameTools.Workers;

public class ModuleBootstrapper : IModuleBootstrapper
{
    public void Bootstrap(IHostApplicationBuilder builder)
    {
        builder.Bootstrap(new LVK.Events.ModuleBootstrapper());
        
        builder.Services.AddHostedService<TimerNotificationsWorker>();
    }
}