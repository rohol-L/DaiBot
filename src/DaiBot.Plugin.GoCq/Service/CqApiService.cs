using DaiBot.Core.Interface;
using Sisters.WudiLib.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Plugin.GoCq.Service
{
    public class CqApiService : CqHttpWebSocketApiClient, IService
    {
        public CqApiService(IBotConfig config):base(config["cq.url"], config["cq.token"])
        {
        }
    }
}
