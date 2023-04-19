using DaiBot.Core;
using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Users.Middleware
{
    public class UsersMiddleware : IMiddleware
    {
        public string Name => "用户信息中间件";

        public int Order => 0;

        readonly DbConnection userDb;

        public UsersMiddleware(IDbCollection dbCollection)
        {
            var db = dbCollection.DefaultDB;
            if (db == null)
            {
                throw new Exception("数据库未初始化");
            }
            userDb = db;
        }

        public void Invoke(MessageContext context, Action next)
        {
            if (context.UserID == 0 || context.GroupID == 0)
            {
                next();
                return;
            }
            try
            {
                var nick = userDb.ExecuteScalar<string?>("select nick from user_info where id=@uid and gid in(@gid,0)", new
                {
                    uid = context.UserID,
                    gid = context.GroupID
                });
                if (nick == null)
                {
                    userDb.Execute("insert user_info(id,gid,nick)values(@id,0,null),(@id,@gid,null)", new
                    {
                        id = context.UserID,
                        gid = context.GroupID
                    });
                }
                else
                {
                    context.UserNick = nick;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[用户信息更新失败]" + ex.Message);
            }
            next();
        }
    }
}
