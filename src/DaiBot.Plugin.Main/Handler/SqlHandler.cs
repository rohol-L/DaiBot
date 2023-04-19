using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using DaiBot.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DaiBot.Plugin.Main.Handler
{
    [AuthorizationFilter("master")]
    [CommandFilter("#query", "#exec", "#insert")]
    internal class SqlHandler : IHandler
    {
        readonly DbConnection mainDB;
        public SqlHandler(IDbCollection dbCollection)
        {
            var db = dbCollection.DefaultDB;
            if (db == null)
            {
                throw new Exception("数据库未初始化");
            }
            mainDB = db;
        }

        public Response? Handle(MessageContext context)
        {
            try
            {
                string cmd = context.CommandPayload[0];
                string payload = context.CommandPayload[1];
                return cmd switch
                {
                    "query" => (Response)Query(payload),
                    "exec" => (Response)Exec(payload),
                    "insert" => (Response)Insert(payload),
                    _ => null,
                };
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        string Query(string sql)
        {
            return mainDB.ExecuteScalar<string>(sql).ToString();
        }

        string Exec(string sql)
        {
            return mainDB.Execute(sql).ToString();
        }

        string Insert(string values)
        {
            string[] arg = values.Split(' ');
            StringBuilder sb = new StringBuilder();
            sb.Append("insert or replace into ");
            sb.Append(arg[0]);
            sb.Append(" values(");
            for (int i = 1; i < arg.Length; i++)
            {
                sb.Append('"');
                sb.Append(arg[i].Replace("\"", "\"\"").Replace("&nbsp;", " "));
                sb.Append("\",");
            }
            sb.Remove(sb.Length - 1, 1);
            sb.Append(");");
            return mainDB.Execute(sb.ToString()).ToString();
        }
    }
}
