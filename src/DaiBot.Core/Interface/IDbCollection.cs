using System.Data.Common;

namespace DaiBot.Core.Interface
{
    public interface IDbCollection
    {
        public DbConnection? this[string name] { get; }

        public DbConnection? DefaultDB { get; }

        public string DefaultDbName { get; }

        public DbConnection Get(string name);
        public DbConnection Add(string name);
    }
}
