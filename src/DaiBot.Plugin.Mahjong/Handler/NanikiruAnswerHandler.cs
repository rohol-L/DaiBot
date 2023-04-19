using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using DaiBot.Plugin.Mahjong;
using DaiBot.Plugin.Mahjong.External;
using DaiBot.Plugin.Mahjong.Models;
using DaiBot.Plugin.Nanikiru.Model;
using Dapper;
using System.Data.Common;

namespace DaiBot.Plugin.Nanikiru.Handler
{
    [CustomerFilter]
    internal class NanikiruAnswerHandler : IHandler
    {
        readonly DbConnection conn;

        public NanikiruAnswerHandler(IDbCollection dbCollection)
        {
            conn = dbCollection.Get(Resource.DbName);
        }

        public Response Handle(MessageContext context)
        {
            string? pid = conn.GetGroupSetting(context.GroupID, "nanikiru_id");
            if (pid == null)
            {
                return Response.Empty;
            }

            string sql = "SELECT * FROM nanikiru where ProblemNumber=" + pid;
            var prob = conn.QueryFirst<NanikiruProblem>(sql);

            var answer = Parse(context.Message);
            if (prob.Answer == null)
            {
                return "脑瓜子乱成了一团，问题编号：" + prob.ProblemNumber;
            }
            if (answer.Kan ^ prob.Answer.StartsWith("kan")
                || answer.Red ^ prob.Answer.Contains('r')
                || !prob.Answer.Contains(answer.Number.ToString())
                || !prob.Answer.Contains(answer.Type))
            {
                return Response.Empty;
            }

            Log(context.UserID, context.GroupID, pid);
            conn.SetGroupSetting(context.GroupID, "nanikiru_id", null);
            return "答案：" + prob.Answer + "\r\n" + prob.ZH ?? prob.Kaisetu;
        }

        private void Log(long uid, long gid, string pid)
        {
            string sql = "insert into nanikiru_answer_log(uid, gid, pid, datetime)"
                + "values(@uid, @gid, @pid, @datetime)";
            conn.Execute(sql, new
            {
                gid,
                uid,
                pid,
                datetime = DateTime.Now,
            });
        }

        const string num_ch = "0123456789零一二三四五六七八九0东南西北白发中";

        static NanikiruAnswer Parse(string answer)
        {
            answer = answer.Trim().ToLower();
            var result = new NanikiruAnswer
            {
                Kan = false,
                Red = false,
                Number = 0,
                Type = '\0'
            };

            int pos = 0;
            if (answer.Length < pos + 1)
            {
                return result;
            }
            if (answer[pos] == '杠')
            {
                result.Kan = true;
                pos++;
            }
            else if (answer.Length > pos + 3 && answer[pos..(pos + 3)] == "kan")
            {
                result.Kan = true;
                pos += 3;
            }

            if (answer.Length < pos + 1)
            {
                return result;
            }
            if (answer[pos] == '红' || answer[pos] == 'r')
            {
                result.Red = true;
                pos++;
            }

            if (answer.Length < pos + 1)
            {
                return result;
            }
            int idx = num_ch.IndexOf(answer[pos]);
            pos++;
            if (idx == 0)
            {
                result.Red = true;
                idx = 5;
            }
            else if (idx <= 0 || idx % 10 == 0)
            {
                return result;
            }
            result.Number = idx % 10;
            if (idx > 20)
            {
                if (pos == answer.Length)
                {
                    result.Type = 'z';
                }
                return result;
            }
            if (pos == answer.Length - 1)
            {
                result.Type = answer[pos] switch
                {
                    '万' => 'm',
                    '条' => 's',
                    '筒' => 'p',
                    '索' => 's',
                    '饼' => 'p',
                    _ => answer[pos],
                };
            }
            return result;
        }

        public static bool Check(MessageContext context)
        {
            string answer = context.Message;
            if (answer.Length < 2 || answer.Length > 6)
            {
                return false;
            }
            return Parse(answer).Type > 0;
        }
    }
}

