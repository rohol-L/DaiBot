using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Mahjong
{
    public class PluginMain : IPlugin
    {
        public string Name => "日麻插件";

        public string Version => "1.0";

        readonly DbConnection mjDb;

        public PluginMain(IDbCollection dbCollection)
        {
            var db = dbCollection[Resource.DbName];
            if (db == null)
            {
                mjDb = dbCollection.Add(Resource.DbName);
                mjDb.Execute(Resource.InitDbSql);
                Console.WriteLine($"[初始化]{Resource.DbName}.db");
            }
            else
            {
                mjDb = db;
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