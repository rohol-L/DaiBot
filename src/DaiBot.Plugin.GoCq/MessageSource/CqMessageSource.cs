using DaiBot.Core;
using DaiBot.Core.Application;
using DaiBot.Core.Interface;
using DaiBot.Plugin.GoCq.Service;
using Sisters.WudiLib;
using Sisters.WudiLib.WebSocket;

namespace DaiBot.Plugin.GoCq.MessageSource
{
    public class CqMessageSource : IMessageSource
    {
        readonly CqApiService cqApi;
        readonly CqHttpWebSocketEvent? cqEvent;
        Action<MessageContext>? send;
        readonly IStorage storage;
        readonly CommandArguments arguments;

        public CqMessageSource(IBotConfig config, IStorage storage, CqApiService cqApiService, CommandArguments arguments)
        {
            this.storage = storage;
            string? url = config["cq.url"];
            if (url == null)
            {
                throw new Exception("缺少配置节点 cq.url");
            }
            string token = config["cq.token"] ?? string.Empty;
            cqApi = cqApiService;
            cqEvent = new CqHttpWebSocketEvent(url, token);
            cqEvent.MessageEvent += CqEvent_MessageEvent;
            this.arguments = arguments;
        }

        public void Start(Action<MessageContext> send)
        {
            if (cqEvent == null || cqApi == null)
            {
                return;
            }
            this.send = send;
            cqEvent.StartListen();
            cqApi.GetLoginInfoAsync().ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine(t.Exception.Message);
                    return;
                }
                storage.Set("qid", t.Result.UserId);
                Console.WriteLine($"QID: {t.Result.UserId}({t.Result.Nickname})");
            });
        }

        private void CqEvent_MessageEvent(HttpApiClient api, Sisters.WudiLib.Posts.Message message)
        {
            if (send == null || cqEvent == null || cqApi == null)
            {
                return;
            }
            var ev = new MessageContext("cq", message.Content.Text, message.RawMessage, rsp =>
            {
                foreach (var item in rsp)
                {
                    if (arguments.DevMode)
                    {
                        Console.WriteLine($"[{message.Endpoint}]{item}");
                    }
                    else
                    {
                        var task = cqApi.SendMessageAsync(message.Endpoint, new RawMessage(item));
                        task.Wait();
                    }
                }
            })
            {
                MessageID = message.MessageId,
                UserID = message.UserId
            };
            if (message is Sisters.WudiLib.Posts.GroupMessage gm)
            {
                ev.GroupID = gm.GroupId;
            }
            send?.Invoke(ev);
        }

        public void Stop()
        {
            send = null;
        }
    }
}