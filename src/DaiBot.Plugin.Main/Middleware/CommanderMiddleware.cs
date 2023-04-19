using DaiBot.Core;
using DaiBot.Core.Interface;

namespace DaiBot.Plugin.Main.Middleware
{
    public class CommanderMiddleware : IMiddleware
    {
        public int Order => 0;

        public string Name => "命令转换中间件";

        readonly IBotConfig config;

        public CommanderMiddleware(IBotConfig config)
        {
            this.config = config;
        }

        public void Invoke(MessageContext context, Action next)
        {
            if (context.Message.StartsWith("#"))
            {
                int pos = context.Message.IndexOf(' ');
                if (pos > 0)
                {
                    context.CommandPayload.Add(context.Message[0..pos]);
                    context.Message = context.Message[(pos + 1)..];
                }
                else
                {
                    context.CommandPayload.Add(context.Message);
                    context.Message = string.Empty;
                }
            }
            if (context.Message.StartsWith(config.BotName + "，")
                || context.Message.StartsWith(config.BotName + " "))
            {
                context.Authorization.Add("at");
                context.Message = context.Message[(config.BotName.Length + 1)..];
            }
            next();
        }
    }
}