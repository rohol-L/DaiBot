using DaiBot.Core;
using DaiBot.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Daibot.Plugin.OpenAI.Middleware
{
    public class ChatMiddleware : IMiddleware
    {
        string IMiddleware.Name => "AiChat中间件";

        int IMiddleware.Order => 1;

        readonly IBotConfig botConfig;


        public ChatMiddleware(IBotConfig botConfig)
        {
            this.botConfig = botConfig;
        }

        public void Invoke(MessageContext context, Action next)
        {
            string name = botConfig.BotName;
            if (context.Message.StartsWith(name + "，") || context.Message.StartsWith(name + " "))
            {
                context.Authorization.Add("at");
                context.Message = context.Message[(name.Length + 1)..];
                context.RawMessage = context.RawMessage[(context.RawMessage.IndexOf(name) + 1)..];
            }
            next();
        }
    }
}
