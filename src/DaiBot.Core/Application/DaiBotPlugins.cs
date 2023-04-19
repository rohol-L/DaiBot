using DaiBot.Core.Interface;

namespace DaiBot.Core.Application
{
    public class DaiBotPlugins
    {
        public List<PluginLoader> PluginList { get; } = new();
        public event EventHandler<PluginLoader> PluginLoaded = (s, e) => { };

        readonly DaiBotApplication app;
        bool Ordered = false;
        private readonly List<IMiddleware> orderedMiddleware = new();

        public DaiBotPlugins(DaiBotApplication app)
        {
            this.app = app;
        }

        public PluginLoader? AddPlugin(string path)
        {
            try
            {
                var plg = PluginLoader.LoadPlugin(path, app.Container);
                PluginList.Add(plg);
                Ordered = false;
                Console.WriteLine($"[插件加载][y]：{plg.Name} {plg.Version}");
                PluginLoaded.Invoke(null, plg);
                return plg;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[插件加载][n]：{path}");
                Exception? inner = ex;
                while (inner != null)
                {
                    Console.WriteLine(inner.Message);
                    inner = inner.InnerException;
                }
            }
            return null;
        }

        public bool RemovePlugin(string name)
        {
            PluginLoader? plg = null;
            foreach (var item in PluginList)
            {
                if (item.Name == name)
                {
                    plg = item;
                    break;
                }
            }
            if (plg == null)
            {
                Console.WriteLine($"[插件卸载][n]：{name} 未注册");
                return false;
            }
            else
            {
                return RemovePlugin(plg);
            }
        }

        public bool RemovePlugin(PluginLoader plg)
        {
            bool result = PluginLoader.UnloadPlugin(plg);
            Console.WriteLine($"[插件卸载][{(result ? 'y' : 'n')}]：{plg.Name}");
            PluginList.Remove(plg);
            Ordered = false;
            return result;
        }

        public IEnumerable<IMessageSource> GetMessageSources()
        {
            return GetEnumerable(plg => plg.MessageSource);
        }

        public IEnumerable<IMiddleware> GetMiddlewares()
        {
            ReOrderMiddleware();
            return orderedMiddleware;
        }

        public IEnumerable<IHandlerBase> GetHandlers()
        {
            return GetEnumerable(plg => plg.Handlers);
        }

        private IEnumerable<T> GetEnumerable<T>(Func<PluginLoader, List<T>> func)
        {
            foreach (var plg in PluginList)
            {
                foreach (var item in func(plg))
                {
                    yield return item;
                }
            }
        }

        private void ReOrderMiddleware()
        {
            if (Ordered)
            {
                return;
            }
            Ordered = true;
            lock (orderedMiddleware)
            {
                orderedMiddleware.Clear();
                foreach (var middleware in GetEnumerable(plg => plg.Middlewares))
                {
                    bool added = false;
                    for (int i = 0; i < orderedMiddleware.Count; i++)
                    {
                        if (orderedMiddleware[i].Order > middleware.Order)
                        {
                            orderedMiddleware.Insert(i, middleware);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        orderedMiddleware.Add(middleware);
                    }
                }
            }
        }
    }
}
