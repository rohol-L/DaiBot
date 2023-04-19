using DaiBot.Core.Interface;
using DaiBot.Core;
using System.Text.RegularExpressions;
using DaiBot.Core.FilterAttributes;
using Daibot.Plugin.Infmonkeys.Service;

namespace Daibot.Plugin.Infmonkeys.Handler
{
    [CommandFilter("#ye18")]
    public class Ye18Handler : IHandlerAsync
    {
        readonly InfmonkeysService service;

        public Ye18Handler(InfmonkeysService service)
        {
            this.service = service;
        }
        public async Task<Response?> HandleAsync(MessageContext context)
        {
            string prompt = context.Message;
            Task<string> task;
            if (Regex.IsMatch(prompt, @"[\u4e00-\u9fa5]"))
            {
                task = service.InferAsync(prompt, "太乙中文模型", "taiyi");
            }
            else
            {
                task = service.InferAsync(prompt, "ye18", "dbe25794e24af183565dc45e9ec99713");
            }


            string id = await task;
            Console.WriteLine("waitID:" + id);
            service.WaitList.Add(id, url =>
            {
                string rsp = $"[CQ:reply,id={context.MessageID}][CQ:image,file=0.jpg,subType=0,url={url}]";
                Console.WriteLine($"[{id}]({service.WaitList.Count})=>{rsp}");
                context.Callback.Invoke(rsp);
                service.WaitList.Remove(id);
            });
            Console.WriteLine($"WaitList:{string.Join(',', service.WaitList.Keys)}");

            return Response.Empty;
        }
    }
}
