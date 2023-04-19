using DaiBot.Core.FilterAttributes;
using DaiBot.Core;
using System.Data.Common;
using Dapper;
using DaiBot.Plugin.Nanikiru.Model;
using DaiBot.Plugin.Nanikiru.Utils;
using SixLabors.ImageSharp;
using DaiBot.Core.Interface;
using DaiBot.Plugin.Mahjong;
using DaiBot.Plugin.Mahjong.External;
using DaiBot.Core.Application;

namespace DaiBot.Plugin.Nanikiru.Handler
{
    [EqualsFilter("nanikiru", "nanikiru!", "何切", "何切!", "何切！")]
    internal class NanikiruProblemHandler : IHandler
    {
        readonly DbConnection conn;
        readonly string pluginPath;

        public NanikiruProblemHandler(IDbCollection dbCollection, PluginLoader plugin)
        {
            conn = dbCollection.Get(Resource.DbName);
            pluginPath = plugin.AssemblyPath;
        }

        public Response Handle(MessageContext context)
        {
            bool passMode = context.Message.EndsWith('!') || context.Message.EndsWith('！');
            string? pid = conn.GetGroupSetting(context.GroupID, "nanikiru_id");

            string where = "where zh is not null";
            if (!passMode && pid != null)
            {
                where = "where ProblemNumber = " + pid;
            }

            string sql = $"SELECT * FROM nanikiru {where} ORDER BY RANDOM() limit 1";
            var nanikiru = conn.QueryFirst<NanikiruProblem>(sql);
            if (nanikiru == null)
            {
                return "没有找到题库。";
            }
            string picPath = @$"cache\nanikiru{nanikiru.ProblemNumber}.png";
            if (!File.Exists(picPath))
            {
                using (var img = MajongHaiHelper.CreatePic(nanikiru, Path.GetDirectoryName(pluginPath) ?? "./"))
                    img.SaveAsPng(picPath);
            }
            conn.SetGroupSetting(context.GroupID, "nanikiru_id", nanikiru.ProblemNumber.ToString());
            picPath = $"file:///{Environment.CurrentDirectory}\\{picPath}";
            return $"[CQ:image,file=0.jpg,subType=0,url={picPath}]";
        }
    }
}
