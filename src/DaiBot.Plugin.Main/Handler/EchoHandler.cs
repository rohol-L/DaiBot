using DaiBot.Core;
using DaiBot.Core.FilterAttributes;
using DaiBot.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Plugin.Main.Handler
{
    [CommandFilter("#echo")]
    public class EchoHandler : IHandler
    {
        public Response? Handle(MessageContext context)
        {
            return context.RawMessage;
        }
    }
}
