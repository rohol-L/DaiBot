using DaiBot.Core;
using DaiBot.Core.Application;
using DaiBot.Core.Interface;
using DaiBot.Core.Utils;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Plugin.Main.Middleware
{
    public class PolicyMiddleware : IMiddleware
    {
        public string Name => "访问策略中间件";

        public int Order => -100;

        readonly CommandArguments arguments;
        readonly IBotConfig config;
        readonly DbConnection mainDB;

        public PolicyMiddleware(CommandArguments arguments, IBotConfig config, IDbCollection dbCollection)
        {
            this.arguments = arguments;
            this.config = config;
            var db = dbCollection.DefaultDB;
            if (db == null)
            {
                throw new Exception("数据库未初始化");
            }
            mainDB = db;
        }

        public void Invoke(MessageContext context, Action next)
        {
            bool run = !arguments.DevMode; // 开发模式默认策略为：不执行
            string uid = context.UserID.ToString();
            string gid = context.GroupID.ToString();

            var userPolicyReader = mainDB.ExecuteReader("select name,value,isAllow from policy where categroy='user' order by eq");
            while (userPolicyReader.Read())
            {
                var name = userPolicyReader["name"].ToString();
                var value = userPolicyReader["value"]?.ToString() ?? string.Empty;
                var users = value.Split(',');
                var isAllow = (long)userPolicyReader["isAllow"];
                if (value == "*" || value.Split(',').Contains(uid))
                {
                    if (isAllow == 1)
                    {
                        //继续验证
                        break;
                    }
                    else
                    {
                        //不执行next
                        MyConsole.WriteLine($"[{name}]拦截：uid={uid},gid={gid}");
                        return;
                    }
                }
                
                // 处理 gid.uid 格式
                foreach (var item in users)
                {
                    int idx = item.IndexOf('.');
                    if (idx > 0)
                    {
                        if (item[0..idx] == gid && item[(idx + 1)..] == uid)
                        {
                            if (isAllow == 1)
                            {
                                //继续验证
                                break;
                            }
                            else
                            {
                                MyConsole.WriteLine($"[{name}]拦截：uid={uid},gid={gid}");
                                //不执行next
                                return;
                            }
                        }
                    }
                }
            }
            userPolicyReader.Close();

            var groupPolicyReader = mainDB.ExecuteReader("select name,value,isAllow from policy where categroy='group' order by eq");
            while (groupPolicyReader.Read())
            {
                var name = groupPolicyReader["name"].ToString();
                var value = groupPolicyReader["value"]?.ToString() ?? string.Empty;
                var isAllow = (int)groupPolicyReader["isAllow"];
                if (value == "*" || value.Split(',').Contains(gid))
                {
                    if (isAllow == 1)
                    {
                        //继续验证
                        break;
                    }
                    else
                    {
                        //不执行next
                        return;
                    }
                }
            }
            groupPolicyReader.Close();

            if (arguments.DebugMode)
            {
                context.Authorization.Add("debug");
            }
            try
            {
                if (run)
                {
                    next();
                    if (arguments.DebugMode)
                    {
                        Console.WriteLine($"[Msg] {context.Message}");
                        Console.WriteLine($"[Auth] {string.Join(',', context.Authorization)}");
                        Console.WriteLine($"[CMD] {string.Join(' ', context.CommandPayload)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                while (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
