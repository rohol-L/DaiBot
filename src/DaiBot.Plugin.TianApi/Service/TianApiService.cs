using DaiBot.Core.Interface;
using DaiBot.Plugin.TianApi.Model;
using System.Net.Http.Json;

namespace DaiBot.Plugin.TianApi.Service
{
    internal class TianApiService : IService
    {
        readonly IBotConfig config;

        public TianApiService(IBotConfig config)
        {
            this.config = config;
        }

        public async Task<TianapiPayload<T>> GetAsync<T>(string path)
        {
            string baseUrl = "https://apis.tianapi.com";
            string? token = config["tianApi.token"];
            if (token == null)
            {
                throw new Exception("tianApi.token is null.");
            }
            HttpClient httpClient = new();
            var rsp = await httpClient.GetFromJsonAsync<TianapiPayload<T>>($"{baseUrl}/{path}?key={token}");
            if (rsp == null)
            {
                throw new Exception("httpClient error");
            }
            return rsp;
        }
    }
}
