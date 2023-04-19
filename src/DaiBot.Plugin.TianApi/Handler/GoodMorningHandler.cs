using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using DaiBot.Plugin.TianApi.Model;
using DaiBot.Plugin.TianApi.Service;

namespace DaiBot.Plugin.TianApi.Handler
{
    [AuthorizationFilter("at")]
    [EqualsFilter("早安", "晚安")]
    internal class GoodMorningHandler : IHandlerAsync
    {
        readonly TianApiService server;
        public GoodMorningHandler(TianApiService server)
        {
            this.server = server;
        }

        public async Task<Response?> HandleAsync(MessageContext context)
        {
            string apiName = context.Message == "晚安" ? "wanan" : "zaoan";
            while (true)
            {
                var result = await server.GetAsync<TianapiContent>($"/{apiName}/index");
                var content = result.Result?.Content;

                if (content == null)
                {
                    Console.WriteLine($"Tianapi:/{apiName}/index error.");
                    return null;
                }
                if (content.Contains('女'))
                {
                    continue;
                }
                return content.Replace('她', '他');
            }
        }
    }
}
