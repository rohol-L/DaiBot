using DaiBot.Core;
using Dapper;
using SixLabors.ImageSharp;
using System.Data.Common;
using DaiBot.Core.FilterAttributes;
using DaiBot.Plugin.Nanikiru.Model;
using DaiBot.Plugin.Nanikiru.Utils;
using DaiBot.Core.Utils;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using DaiBot.Core.Interface;
using DaiBot.Plugin.Mahjong;
using DaiBot.Core.Application;

namespace DaiBot.Plugin.Nanikiru.Handler
{
    [StartWithFilter("何切翻译")]
    internal class NanikiruTranslateHandler : IHandlerAsync
    {
        readonly DbConnection conn;
        readonly IBotConfig botConfig;
        readonly string pluginPath;

        public NanikiruTranslateHandler(IDbCollection dbCollection, IBotConfig botConfig, PluginLoader plugin)
        {
            conn = dbCollection.Get(Resource.DbName);
            this.botConfig = botConfig;
            pluginPath = plugin.AssemblyPath;
        }

        public async Task<Response?> HandleAsync(MessageContext context)
        {
            if (context.Message.StartsWith("何切翻译 "))
            {
                return Update(context.Message);
            }
            else if (context.Message != "何切翻译")
            {
                return null;
            }
            string sql = "select * from nanikiru where zh is null " +
                    "order by FileName,ProblemNumber limit 1";

            var nanikiru = conn.QueryFirst<NanikiruProblem>(sql);
            if (nanikiru == null)
            {
                return "没有找到题库。";
            }
            if (!Directory.Exists("cache"))
            {
                Directory.CreateDirectory("cache");
            }
            string picPath = @$"cache\nanikiru{nanikiru.ProblemNumber}.png";
            using (var img = MajongHaiHelper.CreatePic(nanikiru, Path.GetDirectoryName(pluginPath) ?? "./"))
                img.SaveAsPng(picPath);
            picPath = $"file:///{Environment.CurrentDirectory}\\{picPath}";
            string? raw = nanikiru.ZhAuto ?? nanikiru.Kaisetu;
            if (raw == null)
            {
                return "题库错误：" + nanikiru.ProblemNumber;
            }
            string? result = await TransAsync(raw, "jp");
            string other1 = $"[{nanikiru.Answer}]{nanikiru.ZhAuto}";
            string other2 = $"何切翻译 {nanikiru.ProblemNumber} {result}";
            MyConsole.WriteLine(picPath);
            MyConsole.WriteLine(other1);
            MyConsole.WriteLine(other2);
            return new Response($"[CQ:image,file=0.jpg,subType=0,url={picPath}]{other1}", other2);
        }

        private Response Update(string message)
        {
            string[] args = message.Split(' ');
            string number = args[1];
            string trans = args[2];
            string sql = "update nanikiru set ZH=@trans where ProblemNumber=@number";
            conn.Execute(sql, new { number, trans });
            sql = "select count(ZH) from nanikiru";
            int count = conn.ExecuteScalar<int>(sql);
            return $"{number} {count}/585({585 - count}) {count * 100 / 585f:0.00}%";
        }

        async Task<string?> TransAsync(string q, string from = "en")
        {

            var client = new HttpClient();
            string? appId = botConfig["baiduTrans.appId"];
            string? secretKey = botConfig["baiduTrans.secretKey"];
            if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(secretKey))
            {
                return null;
            }

            string salt = new Random().Next(100000).ToString();
            string sign = EncryptString(appId + q + salt + secretKey);
            q = HttpUtility.UrlEncode(q, Encoding.UTF8);
            string url = $"http://api.fanyi.baidu.com/api/trans/vip/translate?q={q}&from={from}&to=zh&appid={appId}&salt={salt}&sign={sign}";
            var rsp = await client.GetFromJsonAsync<BaiduTransPayload>(url);
            if (rsp == null)
            {
                return null;
            }
            if (rsp.ErrorMsg != null)
            {
                return rsp.ErrorMsg;
            }
            return rsp.TransResult[0].Dst;
        }

        // 计算MD5值
        static string EncryptString(string str)
        {
            MD5 md5 = MD5.Create();
            byte[] byteOld = Encoding.UTF8.GetBytes(str);
            byte[] byteNew = md5.ComputeHash(byteOld);
            StringBuilder sb = new();
            foreach (byte b in byteNew)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
