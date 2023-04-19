using Autofac;
using DaiBot.Core.Application;

namespace DaiBot.Core.Interface
{
    public interface IApplication
    {
        public IContainer Container { get; }

        public DaiBotPlugins Plugins { get; }
    }
}

