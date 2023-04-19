using Daibot.Plugin.Infmonkeys.Service;
using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using Newtonsoft.Json.Linq;

namespace Daibot.Plugin.Infmonkeys.Handler
{
    [CommandFilter("#InfmonkeysCall")]
    [AuthorizationFilter("httppost")]
    internal class CallbackHandler : IHandler
    {
        readonly InfmonkeysService service;

        public CallbackHandler(InfmonkeysService service)
        {
            this.service = service;
        }

        public Response? Handle(MessageContext context)
        {
            string? json = context.OtherPayload?.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return "error";
            }
            var obj = JObject.Parse(json);
            int? status = obj["status"]?.Value<int>();
            if (status == null || status == 3)
            {
                Console.WriteLine("error:" + json);
                return "error";
            }
            else if (status == 2)
            {
                string? id = obj["id"]?.ToString();
                string? href = obj["hrefs"]?[0]?.ToString();
                if (id == null)
                {
                    Console.WriteLine($"id:{id},href:{href}");
                }
                else if (service.WaitList.TryGetValue(id, out Action<string>? action))
                {
                    if (href == null)
                    {
                        action.Invoke("脑壳坏掉了，画画失败。");
                    }
                    else
                    {
                        action.Invoke(href);
                    }
                }
                else
                {
                    Console.WriteLine($"ID?={id} {json}");
                }
            }
            return "success";
        }
    }
}
