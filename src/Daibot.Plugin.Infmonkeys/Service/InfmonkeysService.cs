using DaiBot.Core.Interface;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibot.Plugin.Infmonkeys.Service
{
    public class InfmonkeysService : IService
    {
        readonly string team;
        readonly string token;

        public Dictionary<string, Action<string>> WaitList { get; } = new();

        public InfmonkeysService(IBotConfig config)
        {
            team = config["infomonkeys.team"] ?? string.Empty;
            token = config["infomonkeys.token"] ?? string.Empty;
        }

        public async Task<string> InferAsync(string prompt, string baseModel, string modelId)
        {
            var dict = ParseMsg(prompt);

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("team", team);
            client.DefaultRequestHeaders.Add("token", token);
            var body = new
            {
                type = 1,
                baseModel,
                modelId,
                prompt = dict["Prompt"],
                negativePrompt = dict["Negative prompt"],
                name = "DaiBot Infer",
                samplingStep = int.Parse(dict["Steps"]),
                cfgScale = float.Parse(dict["CFG scale"]),
                width = int.Parse(dict["Width"]),
                height = int.Parse(dict["Height"]),
                batchCount = 1,
                batchSize = 1,
                seed = int.Parse(dict["Seed"])
            };

            var rsp = await client.PostAsJsonAsync("https://frame-api.infmonkeys.com/api/aigc/infer", body);
            string result = await rsp.Content.ReadAsStringAsync();
            var match = new Regex(@"[\da-f]{24}").Match(result);
            if (match.Success)
            {
                return match.Groups[0].Value;
            }
            else
            {
                throw new Exception(result);
            }
        }

        private static Dictionary<string, string> ParseMsg(string msg)
        {
            var data = new Dictionary<string, string>();
            string key = "Prompt";
            string value = "";
            var sb = new StringBuilder();
            int qt = 0;
            for (int i = 0; i < msg.Length; i++)
            {
                if (msg[i] == '\r' || msg[i] == '\n' || msg[i] == ',')
                {
                    if (msg[i] == ',' && (key == "Prompt" || key == "Negative prompt"))
                    {
                        sb.Append(',');
                    }
                    value += sb.ToString();
                    sb.Clear();
                    continue;
                }
                if (msg[i] == ':' && qt == 0)
                {
                    data.Add(key.Trim(), value.Trim());
                    key = sb.ToString();
                    value = "";
                    sb.Clear();
                    continue;
                }
                sb.Append(msg[i]);
                if (msg[i] == '(')
                {
                    qt++;
                }
                if (msg[i] == ')')
                {
                    qt--;
                }
            }
            if (key == "Prompt")
            {
                value += sb.ToString();
                data.Add(key.Trim(), value.Trim());
            }
            else
            {
                data.Add(key.Trim(), sb.ToString().Trim());
            }
            Console.WriteLine(data["Prompt"]);
            Console.WriteLine(string.Join(',', data.Keys));
            if (!data.ContainsKey("Negative prompt"))
            {
                data.Add("Negative prompt", "Simple background, blurry, dark, female, vaginal,penetration");
            }
            if (!data.ContainsKey("Steps"))
            {
                data.Add("Steps", "20");
            }
            if (!data.ContainsKey("Sampler"))
            {
                data.Add("Sampler", "Euler a");
            }
            if (!data.ContainsKey("CFG scale"))
            {
                data.Add("CFG scale", "7");
            }
            if (!data.ContainsKey("Seed"))
            {
                data.Add("Seed", "-1");
            }
            if (!data.ContainsKey("Width"))
            {
                data.Add("Width", "512");
            }
            if (!data.ContainsKey("Height"))
            {
                data.Add("Height", "512");
            }
            return data;
        }
    }
}
