using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Users.Handler
{
    [CommandFilter("#userNick")]
    [AuthorizationFilter("master")]
    public class SetUserNickHandler : IHandler
    {
        readonly DbConnection userDb;

        public SetUserNickHandler(IDbCollection dbCollection)
        {
            var db = dbCollection.DefaultDB;
            if (db == null)
            {
                throw new Exception("数据库未初始化");
            }
            userDb = db;
        }

        public Response? Handle(MessageContext context)
        {
            int count = context.CommandPayload.Count;
            if (count == 1)
            {
                return "未标记用户：" + userDb.ExecuteScalar<int>("select count(1) from user_info where nick is null");
            }
            string uid = context.CommandPayload[1];
            if (count == 2)
            {
                string? info = userDb.ExecuteScalar<string?>("select group_concat(gid||':'||nick) from user_info where id = @uid", new { uid });
                if (info == null)
                {
                    return "没有用户信息：" + uid;
                }
                else
                {
                    return info.Replace(',', '\n');
                }

            }
            long gid = 0;
            string nick;
            if (context.CommandPayload.Count == 3)
            {
                nick = context.CommandPayload[2];
            }
            else if (context.CommandPayload.Count == 4)
            {
                if (long.TryParse(context.CommandPayload[2], out gid))
                {
                    nick = context.CommandPayload[2];
                }
                else
                {
                    return "参数3应为数字";
                }
            }
            else
            {
                return "参数个数有误";
            }
            int result = userDb.Execute("insert or replace into user_info values(@uid,@gid,@nick)", new { uid, gid, nick });
            return "ok:" + result;
        }
    }
}
