using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using Dapper;
using System.Data.Common;
using System.Text.RegularExpressions;

namespace Daibot.Plugin.OpenAI.Handler
{
    [AuthorizationFilter("at")]
    [RegexFilter(@"记住(\w+)[:：].*\1.*")]
    public class UserPromptHandler : IHandler
    {
        readonly DbConnection chatDB;
        public UserPromptHandler(IDbCollection dbCollection)
        {
            chatDB = dbCollection.Get(Resource.DbName);
        }

        public Response? Handle(MessageContext context)
        {
            var reg = new Regex(@"记住(\w+)[:：](.*\1.*)");
            Match match = reg.Match(context.Message);
            if (!match.Success)
            {
                return null;
            }
            string key = match.Groups[1].Value;
            string value = match.Groups[2].Value.Trim();
            if (value[^1] != '。')
            {
                value += "。";
            }

            if (value.Length <= key.Length)
            {
                return null;
            }
            if (value.Length > 100)
            {
                return $"[CQ:reply,id={context.MessageID}]哇，你的说明太长了。记不住啊";
            }

            chatDB.Execute("insert or replace into prompt_rule(gid,name,prompt) values(@gid,@name,@prompt) ", new
            {
                gid = context.GroupID,
                name = key,
                prompt = value
            });
            return $"[CQ:reply,id={context.MessageID}]我记住了，以后提起{key}，我就知道是什么啦。";
        }
    }
}
