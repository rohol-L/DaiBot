using DaiBot.Core;
using DaiBot.Core.Delegates;
using DaiBot.Core.Interface;
using DaiBot.Core.Utils;
using System.Reflection;

namespace DaiBot.Plugin.Main.MessageSource
{
    public class CmdMessageSource : IMessageSource
    {
        Action<MessageContext>? send;
        readonly OnMessageDelegate onMessage;

        public CmdMessageSource(IBotConfig botConfig)
        {
            onMessage = message =>
            {
                var ev = new MessageContext("cmd", message, message, rsp =>
                {
                    foreach (var item in rsp)
                    {
                        Console.WriteLine("==>" + item);
                    }
                })
                {
                    GroupID = botConfig.Value<long>("cmd.gid"),
                    UserID = botConfig.Value<long>("cmd.uid"),
                };
                string? authorization = botConfig["cmd.authorization"];
                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    foreach (var auth in authorization.Split(','))
                    {
                        ev.Authorization.Add(auth);
                    }
                }
                send?.Invoke(ev);
            };
        }

        public void Start(Action<MessageContext> send)
        {
            this.send = send;
            MyConsole.Receiver.Add(onMessage);
        }

        public void Stop()
        {
            MyConsole.Receiver.Remove(onMessage);
            send = null;
        }
    }
}