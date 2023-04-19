using DaiBot.Core.Application;
using DaiBot.Core.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DaiBot.Service
{
    public class BotConfigService : IBotConfig
    {
        private JObject cfg;
        const string fileName = "config.json";

        public string BotName => this["botName"] ?? "!unknow";

        public BotConfigService(CommandArguments arguments)
        {
            string json = File.ReadAllText(fileName);
            cfg = JObject.Parse(json);
            if (arguments.DebugMode)
            {
                string json2 = File.ReadAllText("config_dev.json");
                JObject cfg2 = JObject.Parse(json2);
                cfg.Merge(cfg2);
            }
        }

        private JToken? GetNode(string key)
        {
            string[] args = key.Split('.');
            JToken? node = cfg;
            for (int i = 0; i < args.Length; i++)
            {
                node = node[args[i]];
                if (node == null)
                {
                    return null;
                }
            }
            return node;
        }

        public string? this[string key] => GetNode(key)?.ToString();

        public List<string> GetList(string key)
        {
            JToken? node = GetNode(key);
            var list = new List<string>();
            if (node == null)
            {
                return list;
            }
            if (node.Type == JTokenType.Array)
            {
                foreach (JToken item in node)
                {
                    list.Add(item.ToString());
                }
                return list;
            }
            list.Add(node.ToString());
            return list;
        }

        public void Save()
        {
            File.WriteAllText(fileName, cfg.ToString(Formatting.Indented));
        }

        public void ReLoad()
        {
            string json = File.ReadAllText(fileName);
            cfg = JObject.Parse(json);
        }

        public T? Value<T>(string key)
        {
            JToken? node = GetNode(key);
            if (node == null)
            {
                return default;
            }
            return node.Value<T>();
        }
    }
}
