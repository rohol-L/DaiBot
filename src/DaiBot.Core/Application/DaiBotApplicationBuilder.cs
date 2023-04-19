using Autofac;
using Autofac.Core;
using DaiBot.Core.Interface;
using DaiBot.Core.Utils;
using System.Reflection;

namespace DaiBot.Core.Application
{
    public class DaiBotApplicationBuilder
    {
        public DaiBotServices Services { get; init; }

        internal DaiBotApplicationBuilder(string[] args)
        {
            Services = new DaiBotServices();
            Services.AddSingleton(new CommandArguments(args));
        }

        public DaiBotApplication Build()
        {
            var app = new DaiBotApplication();
            Services.AddSingleton<IApplication>(app);
            var container = Services.Build();
            app.Container = container;
            return app;
        }
    }
}