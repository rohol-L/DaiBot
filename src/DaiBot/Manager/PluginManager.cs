using DaiBot.Core;
using DaiBot.Core.Application;
using DaiBot.Core.Utils;

namespace DaiBot.Manager
{
    public static class PluginManager
    {
        private static DaiBotPlugins? daiBotPlugins;

        public static void Start(DaiBotPlugins daiBotPlugins)
        {
            if (PluginManager.daiBotPlugins == null)
            {
                PluginManager.daiBotPlugins = daiBotPlugins;
                MyConsole.Receiver.Add(message =>
                {
                    if (message == "#plugin reload")
                    {
                        ReloadPlugin();
                    }
                    else if (message == "#plugin unload")
                    {
                        UnloadAllPlugin();
                    }
                });
            }
            else
            {
                throw new Exception("不允许重复执行");
            }
        }

        public static void ReloadPlugin()
        {
            if (daiBotPlugins == null)
            {
                throw new Exception("未调用Start函数");
            }
            var assPath = new List<string>();
            foreach (var pluginLoader in daiBotPlugins.PluginList.ToArray())
            {
                FileInfo fi = new(pluginLoader.AssemblyPath);
                if (!fi.Exists || fi.LastWriteTime != pluginLoader.LastWriteTime)
                {
                    daiBotPlugins.RemovePlugin(pluginLoader);
                }
                else
                {
                    assPath.Add(pluginLoader.AssemblyPath.ToLower());
                }
            }
            var directoryInfo = new DirectoryInfo("./Plugins");
            foreach (FileInfo fi in directoryInfo.GetFiles("DaiBot.Plugin.*.dll", SearchOption.AllDirectories))
            {
                if (assPath.Contains(fi.FullName.ToLower()))
                {
                    continue;
                }
                daiBotPlugins.AddPlugin(fi.FullName);
            }
        }

        public static void UnloadAllPlugin()
        {
            if (daiBotPlugins == null)
            {
                throw new Exception("未调用Start函数");
            }
            foreach (var pluginLoader in daiBotPlugins.PluginList.ToArray())
            {
                daiBotPlugins.RemovePlugin(pluginLoader);
            }
        }
    }
}
