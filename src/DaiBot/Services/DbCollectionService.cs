using DaiBot.Core.Interface;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Data.SQLite;

namespace DaiBot.Services
{
    class DbCollectionService : IDbCollection
    {
        private ConcurrentDictionary<string, DbConnection> dbDict = new();

        public DbConnection? DefaultDB => this["main"];

        public string DefaultDbName => "main";

        public DbCollectionService()
        {
            foreach (var item in Directory.GetFiles(".\\Datas", "*.db"))
            {
                Add(Path.GetFileNameWithoutExtension(item));
            }
            if (dbDict.ContainsKey("main"))
            {

            }
        }

        public DbConnection? this[string name]
        {
            get
            {
                if (dbDict.TryGetValue(name, out var db))
                {
                    return db;
                }
                return null;
            }
        }

        public DbConnection Get(string name)
        {
            if (dbDict.TryGetValue(name, out var db))
            {
                return db;
            }
            throw new Exception("数据库未初始化:" + name);
        }

        public DbConnection Add(string name)
        {
            if (dbDict.TryGetValue(name, out var db))
            {
                return db;
            }
            SQLiteConnection conn = new(@$"data source=.\Datas\{name}.db");
            dbDict.TryAdd(name, conn);
            return conn;
        }
    }
}
