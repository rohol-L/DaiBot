using Autofac;
using DaiBot.Core.Interface;

namespace DaiBot.Core.Application
{
    public class DaiBotApplication : IApplication
    {
        private IContainer? container;

        public IContainer Container
        {
            get
            {
                if (container == null)
                {
                    throw new NullReferenceException("container is null.");
                }
                return container;
            }
            internal set
            {
                container = value;
            }
        }

        public DaiBotPlugins Plugins { get; }

        internal DaiBotApplication()
        {
            Plugins = new(this);
        }

        public static DaiBotApplicationBuilder CreateBuilder(params string[] args)
        {
            return new DaiBotApplicationBuilder(args);
        }

        public void Start()
        {
            foreach (var ms in Plugins.GetMessageSources())
            {
                ms?.Start(Distribute);
            }
            Plugins.PluginLoaded += (s, e) =>
            {
                foreach (var ms in e.MessageSource)
                {
                    ms?.Start(Distribute);
                }
            };
        }

        void Distribute(MessageContext context)
        {
            using var scope = Container.BeginLifetimeScope();
            context.Scope = scope;
            var me = Plugins.GetMiddlewares().GetEnumerator();
            void Invoke()
            {
                if (me.MoveNext())
                {
                    var middleware = me.Current;
                    if (middleware.Order != -100 && context.Authorization.Contains("debug"))
                    {
                        Console.WriteLine($"[中间件]{middleware.Order}:{middleware.Name} {context.Message}");
                    }
                    middleware.Invoke(context, Invoke);
                }
            };
            Invoke();
        }
    }
}
