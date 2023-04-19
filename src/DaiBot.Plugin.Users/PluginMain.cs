using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Users
{
    public class PluginMain : IPlugin
    {
        public string Name => "用户管理插件";

        public string Version => "1.0";

        public void OnLoad()
        {

        }

        public void OnUnload()
        {

        }
    }
}