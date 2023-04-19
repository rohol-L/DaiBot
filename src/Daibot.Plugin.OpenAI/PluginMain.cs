using Daibot.Plugin.OpenAI;
using Daibot.Plugin.OpenAI.Utils;
using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Users
{
    public class PluginMain : IPlugin
    {
        public string Name => "OpenAI插件";

        public string Version => "1.0";

        readonly DbConnection userDb;

        public PluginMain(IDbCollection dbCollection)
        {
            var db = dbCollection[Resource.DbName];
            if (db == null)
            {
                userDb = dbCollection.Add(Resource.DbName);
                userDb.Execute(Resource.InitDbSql);
                Console.WriteLine($"[初始化]{Resource.DbName}.db");
            }
            else
            {
                userDb = db;
            }
        }
        public void OnLoad()
        {
        }

        public void OnUnload()
        {

        }
    }
}