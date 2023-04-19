using Daibot.Plugin.OpenAI.Utils;
using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using DaiBot.Core.Utils;
using Dapper;
using Newtonsoft.Json.Linq;
using System.Data.Common;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibot.Plugin.OpenAI.Handler
{
    [AuthorizationFilter("at")]
    [DefaultFilter]
    public class GPTv3Handler : IHandlerAsync
    {
        readonly IBotConfig botConfig;
        readonly DbConnection chatDB;

        public GPTv3Handler(IBotConfig botConfig, IDbCollection dbCollection)
        {
            this.botConfig = botConfig;
            chatDB = dbCollection.Get(Resource.DbName);
        }

        public async Task<Response?> HandleAsync(MessageContext context)
        {
            var storyName = GetStoryName(context.GroupID);
            string? groupScene = GetStory(storyName);
            if (string.IsNullOrWhiteSpace(groupScene))
            {
                Console.WriteLine("找不到场景：" + context.GroupID);
                return null;
            }

            long uid = context.UserID;
            long gid = context.GroupID;
            string bot = botConfig.BotName;
            string nick = context.UserNick ?? "好朋友";
            string message = context.Message;
            string stop = $"{nick}：|{bot}：";
            StringBuilder promptBuilder = new();

            string weekdays = "日一二三四五六";
            Dictionary<string, string> vars = new()
            {
                { "bot", bot },
                { "nick", nick },
                { "master", botConfig["masterName"]??"呆狼" },
                { "story", storyName },
                { "tody", DateTime.Now.ToString("yyyy-MM-dd") },
                { "now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                { "week", weekdays[(int)DateTime.Now.DayOfWeek].ToString()},
            };

            string Replace(string input)
            {
                return Regex.Replace(input, @"{\w+}", (m) =>
                {
                    string key = m.Value.Trim('{', '}');
                    if (vars.TryGetValue(key, out string? value))
                    {
                        return value;
                    }
                    else
                    {
                        return m.Value;
                    }
                });
            }

            promptBuilder.AppendLine(Replace(groupScene));

            var reader = chatDB.ExecuteReader("select name,prompt,rule from prompt_rule");
            var rc = new RuleChecker(message);
            rc.AddFunction("var", exps =>
            {
                if (exps.Count == 0)
                    return RuleChecker.Exp.FalseExp;
                string? key = exps[0].Value;
                if (key != null && vars.TryGetValue(key, out string? value))
                {
                    return new RuleChecker.Exp() { Value = value };
                }
                return RuleChecker.Exp.FalseExp;
            });
            rc.AddFunction("auth", exps =>
            {
                if (exps.Count == 0)
                    return RuleChecker.Exp.FalseExp;
                string? key = exps[0].Value;
                if (key != null && context.Authorization.Contains(key))
                {
                    return RuleChecker.Exp.TrueExp;
                }
                return RuleChecker.Exp.FalseExp;
            });
            while (reader.Read())
            {
                string? name = reader["prompt"]?.ToString();
                string? prompt = reader["prompt"]?.ToString();
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(prompt))
                {
                    continue;
                }
                string? rule = reader["rule"]?.ToString();
                if (string.IsNullOrWhiteSpace(rule))
                {
                    if (message.Contains(name, StringComparison.OrdinalIgnoreCase))
                    {
                        promptBuilder.AppendLine(Replace(prompt));
                    }
                }
                else if (rc.Check(rule))
                {
                    promptBuilder.AppendLine(Replace(prompt));
                }
            }

            string? talkPrompt = GetTalkPrompt(storyName);
            if (talkPrompt == null)
            {
                promptBuilder.AppendLine($"以下是{bot}与{nick}的对话。");
            }
            else
            {
                promptBuilder.AppendLine(Replace(talkPrompt));
            }
            promptBuilder.AppendLine();

            //生成对话
            string? dialog = GetReplyText(context.RawMessage);
            if (!string.IsNullOrWhiteSpace(dialog))
            {
                promptBuilder.AppendLine($"{bot}：{dialog}");
            }
            promptBuilder.AppendLine($"{nick}：{context.Message}");
            promptBuilder.AppendLine($"{bot}：");

            string gptPrompt = promptBuilder.ToString();

            MyConsole.WriteLine(">>>GPT3>>>");
            MyConsole.WriteLine(gptPrompt);
            MyConsole.WriteLine("===GPT3===");

            string? result = await Completion(gptPrompt, stop);
            MyConsole.WriteLine(result);
            MyConsole.WriteLine("===GPT3===");

            if (context.MessageID != 0 && context.GroupID != 0)
            {
                return $"[CQ:reply,id={context.MessageID}]{result?.Trim()}";
            }

            return result?.Trim();
        }

        private string GetStoryName(long gid)
        {
            string sql = "select story_name from group_story where gid=@gid and gid in(@gid,0) order by gid desc limit 1";
            string storyName = chatDB.ExecuteScalar<string?>(sql, new { gid }) ?? "default";
            return storyName;
        }

        private string? GetStory(string storyName)
        {
            string sql = "select story from story_list where name = @storyName";
            return chatDB.ExecuteScalar<string?>(sql, new { storyName });
        }

        private string? GetTalkPrompt(string storyName)
        {
            string sql = "select talk_prompt from story_list where name = @storyName";
            return chatDB.ExecuteScalar<string?>(sql, new { storyName });
        }

        private async Task<string?> Completion(string prompt, string stop)
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + botConfig["openAI.token"]);
            var rsp = await httpClient.PostAsJsonAsync("https://api.openai.com/v1/completions", new
            {
                model = "text-davinci-003",
                prompt = $"{prompt}",
                max_tokens = 2000,
                temperature = 0.9f,
                top_p = 1,
                frequency_penalty = 0,
                presence_penalty = 0.6f,
                stop = stop.Split('|')
            });

            string resultStr = await rsp.Content.ReadAsStringAsync();
            var result = JObject.Parse(resultStr);
            string? text = result?["choices"]?[0]?["text"]?.Value<string>();
            if (string.IsNullOrEmpty(text))
            {
                string? error = result?["error"]?["message"]?.Value<string>();
                if (error == null)
                {
                    throw new Exception(resultStr);
                }
                else
                {
                    return error;
                }
            }
            return text;
        }

        /// <summary>
        /// 获取回复的Message
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        private static string? GetReplyText(string raw)
        {
            var regex = new Regex(@"\[CQ:reply,.*text=(.*?)\]");
            Match match = regex.Match(raw);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }
    }
}
