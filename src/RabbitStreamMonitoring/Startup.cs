using Autofac;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitStreamMonitoring.Configuration;
using RabbitStreamMonitoring.Service;
using Swisschain.Sdk.Server.Common;

namespace RabbitStreamMonitoring
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration)
            : base(configuration)
        {
        }
        
        protected override void ConfigureContainerExt(ContainerBuilder builder)
        {
            builder.RegisterType<MonitoringManager>()
                .As<IStartable>()
                .AsSelf()
                .SingleInstance()
                .AutoActivate();

        }
    }
}
