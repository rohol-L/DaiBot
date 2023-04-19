using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Main
{
    public class PluginMain : IPlugin
    {
        public string Name => "核心插件";

        public string Version => "1.0";

        readonly DbConnection mainDb;

        public PluginMain(IDbCollection dbCollection)
        {
            var db = dbCollection.DefaultDB;
            if (db == null)
            {
                mainDb = dbCollection.Add(dbCollection.DefaultDbName);
                mainDb.Execute(Resource.InitDbSql);
                Console.WriteLine($"[初始化]{dbCollection.DefaultDbName}.db");
            }
            else
            {
                mainDb = db;
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