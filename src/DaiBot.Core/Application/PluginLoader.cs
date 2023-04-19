using Autofac;
using Autofac.Builder;
using Autofac.Core;
using DaiBot.Core.Interface;
using DaiBot.Core.LifetimeAttributes;
using DaiBot.Core.Utils;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

namespace DaiBot.Core.Application
{
    public class PluginLoader : IService
    {
        public string Name { get; private set; } = "unknow";
        public string Version { get; private set; } = "unknow";
        public DateTime LastWriteTime { get; private set; }

        public string AssemblyPath { get; init; }

        private WeakReference? weakReference;
        public IPlugin? Plugin;
        public List<IMessageSource> MessageSource { get; } = new();
        public List<IHandlerBase> Handlers { get; } = new();
        public List<IMiddleware> Middlewares { get; } = new();

        private ContainerBuilder containerBuilder = new();
        private IContainer? container = null;

        public IContainer Container
        {
            get
            {
                if (container == null)
                {
                    throw new Exception("未初始化容器");
                }
                return container;
            }
        }

        MemoryStream? ms;

        public bool UnloadState { get; private set; } = false;

        public PluginLoader(string path)
        {
            AssemblyPath = path;
        }

        public static PluginLoader LoadPlugin(string path, ILifetimeScope scope)
        {
            var pl = new PluginLoader(path);
            pl.Load(scope);
            return pl;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Load(ILifetimeScope scope)
        {
            var fileInfo = new FileInfo(AssemblyPath);
            if (!fileInfo.Exists)
            {
                Console.WriteLine("没有找到插件：" + AssemblyPath);
                return;
            }
            LastWriteTime = fileInfo.LastWriteTime;

            var alc = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(AssemblyPath), true);
            //依赖的类库放在插件同级lib目录下
            alc.Resolving += (context, name) =>
            {
                string path = Path.Combine(fileInfo.DirectoryName ?? string.Empty, "lib", name.Name + ".dll");
                return context.LoadFromAssemblyPath(path);
            };
            weakReference = new WeakReference(alc);
            ms = new MemoryStream(File.ReadAllBytes(AssemblyPath));
            var ass = alc.LoadFromStream(ms);
            foreach (var assType in ass.GetTypes())
            {
                LoadService(assType, scope);
            }
            containerBuilder.RegisterInstance(this).AsSelf().SingleInstance();
            container = containerBuilder.Build();
            foreach (var assType in ass.GetTypes())
            {
                LoadPluginInfo(assType, scope);
                LoadMessageSource(assType, scope);
                LoadMiddleware(assType, scope);
                LoadHander(assType, scope);
            }
            Plugin?.OnLoad();
        }

        private void LoadService(Type assType, ILifetimeScope scope)
        {
            var iServiceType = assType.GetInterface(nameof(IService));
            if (iServiceType == null)
            {
                return;
            }
            // 支持多种生命周期
            var lifetimeAttr = iServiceType.GetCustomAttribute<LifetimeAttribute>();
            if (lifetimeAttr is TransientInstanceAttribute)
            {
                containerBuilder.Register(c =>
                {
                    if (MyInject.CreateInstance(assType, scope, new List<IService>()) is IService service)
                    {
                        return (object)service;
                    }
                    throw new Exception("解析异常");
                }).As(assType).InstancePerDependency();
            }
            else if (lifetimeAttr is ScopedInstanceAttribute)
            {
                containerBuilder.Register(c =>
                {
                    if (MyInject.CreateInstance(assType, scope, new List<IService>()) is IService service)
                    {
                        return (object)service;
                    }
                    throw new Exception("解析异常");
                }).As(assType).InstancePerLifetimeScope();
            }
            else
            {
                containerBuilder.Register(c =>
                {
                    if (MyInject.CreateInstance(assType, scope, new List<IService>()) is IService service)
                    {
                        return (object)service;
                    }
                    throw new Exception("解析异常");
                }).As(assType).SingleInstance();
            }
        }

        private void LoadPluginInfo(Type assType, ILifetimeScope scope)
        {
            var iMiddlewareType = assType.GetInterface(nameof(IPlugin));
            if (iMiddlewareType == null)
            {
                return;
            }
            if (MyInject.CreateInstance(assType, scope, Container) is IPlugin iPlugin)
            {
                Plugin = iPlugin;
                Name = iPlugin.Name;
                Version = iPlugin.Version;
            }
            return;
        }

        private void LoadMessageSource(Type assType, ILifetimeScope scope)
        {
            var iMessageSourceType = assType.GetInterface(nameof(IMessageSource));
            if (iMessageSourceType == null)
            {
                return;
            }
            if (MyInject.CreateInstance(assType, scope, Container) is IMessageSource iMessageSource)
            {
                MessageSource.Add(iMessageSource);
            }
        }

        private void LoadMiddleware(Type assType, ILifetimeScope scope)
        {
            var iMiddlewareType = assType.GetInterface(nameof(IMiddleware));
            if (iMiddlewareType == null)
            {
                return;
            }
            if (MyInject.CreateInstance(assType, scope, Container) is IMiddleware iMiddleware)
            {
                Middlewares.Add(iMiddleware);
            }
        }

        private void LoadHander(Type assType, ILifetimeScope scope)
        {
            var iHandlerType = assType.GetInterface(nameof(IHandlerBase));
            if (iHandlerType == null)
            {
                return;
            }
            if (MyInject.CreateInstance(assType, scope, Container) is IHandlerBase iHandler)
            {
                Handlers.Add(iHandler);
            }
        }

        private WeakReference? UnloadAssembly()
        {
            foreach (var item in MessageSource)
            {
                item?.Stop();
            }
            MessageSource.Clear();
            Middlewares.Clear();
            Handlers.Clear();
            Plugin?.OnUnload();
            Plugin = null;
            ms?.Close();
            ms?.Dispose();
            if (weakReference == null)
            {
                return null;
            }
            if (!UnloadState && weakReference.Target is AssemblyLoadContext context)
            {
                context.Unload();
            }
            UnloadState = true;
            return weakReference;
        }

        public static bool UnloadPlugin(PluginLoader pluginLoader)
        {
            var weakReference = pluginLoader.UnloadAssembly();
            if (weakReference == null)
            {
                return true;
            }
            for (int i = 0; weakReference.IsAlive && i < 10; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            return !weakReference.IsAlive;
        }

        public string GetMiddlewaresOrders()
        {
            var stringBuilder = new StringBuilder();
            foreach (var item in Middlewares)
            {
                stringBuilder.Append(item.Name);
                stringBuilder.Append('=');
                stringBuilder.Append(item.Order);
                stringBuilder.Append(',');
            }
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
            return stringBuilder.ToString();
        }
    }
}
