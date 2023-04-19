using DaiBot.Core;
using DaiBot.Core.Interface;
using DaiBot.Plugin.GoCq.Service;
using Dapper;
using System.ComponentModel.Design.Serialization;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace DaiBot.Plugin.GoCq.Middleware
{
    public class CqMiddleware : IMiddleware
    {
        public string Name => "Cq消息预处理中间件";

        public int Order => 1;

        readonly IStorage storage;
        readonly IDbCollection dbCollection;
        readonly CqApiService cqApiService;

        public CqMiddleware(IStorage storage, IDbCollection dbCollection, CqApiService cqApiService)
        {
            this.storage = storage;
            this.dbCollection = dbCollection;
            this.cqApiService = cqApiService;
        }

        public void Invoke(MessageContext context, Action next)
        {
            //自动采集昵称
            if (context.GroupID != 0 && context.UserNick == null)
            {
                context.UserNick = GetNick(context.UserID, context.GroupID);
            }
            string? qid = storage.GetString("qid");//botqq
            if (qid == null)
            {
                next();
                return;
            }
            string raw = context.RawMessage.Trim();
            DbConnection? userDb = dbCollection["user"];
            //识别 @at
            var regex_at = new Regex(@"\[CQ:at,qq=(\d+)\]", RegexOptions.Multiline);
            context.RawMessage = regex_at.Replace(raw, match =>
            {
                string at_qid = match.Groups[1].Value;
                if (match.Index == 0 && at_qid == qid)
                {
                    context.Authorization.Add("at");
                    return string.Empty;
                }
                if (userDb != null)
                {
                    string? nick = userDb.ExecuteScalar<string?>("select nick from user_info where id=@uid and gid in(0,@gid)", new
                    {
                        uid = context.UserID,
                        gid = context.GroupID
                    });
                    if (nick == null)
                    {
                        return GetNick(context.UserID, context.GroupID) ?? string.Empty;
                    }
                    else
                    {
                        return nick;
                    }
                }
                return string.Empty;
            });
            next();
        }

        static readonly Regex ch_reg = new(@"[\u4e00-\u9fa5]{2,}");
        static readonly Regex wd_reg = new(@"\d{2,}");
        private string? GetNick(long qid, long gid)
        {
            var task = cqApiService.GetGroupMemberInfoAsync(gid, qid);
            task.Wait();
            if (task.Exception == null)
            {
                var nick = task.Result.InGroupName;
                var m = ch_reg.Match(nick);
                if (m.Success)
                {
                    return m.Groups[0].Value;
                }
                m = wd_reg.Match(nick);
                if (m.Success)
                {
                    return m.Groups[0].Value;
                }
            }
            return null;
        }
    }
}
