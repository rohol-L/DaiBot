using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Mahjong.External
{
    internal static class DbSetting
    {
        public static string? GetGroupSetting(this DbConnection conn, long gid, string name)
        {
            return conn.ExecuteScalar<string?>("select value from setting where gid = @gid and name = @name and uid = 0", new
            {
                gid,
                name
            });
        }

        public static void SetGroupSetting(this DbConnection conn, long gid, string name, string? value)
        {
            conn.Execute("INSERT OR REPLACE INTO setting(gid,uid,name,value) values(@gid,0,@name,@value)", new
            {
                gid,
                name,
                value
            });
        }
    }
}
