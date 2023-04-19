using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Application
{
    public class CommandArguments
    {
        public bool DebugMode { get; }

        public bool DevMode { get; }

        public CommandArguments(string[] args)
        {
            DebugMode = args.Contains("--debug");
            DevMode = args.Contains("--dev");
        }
    }
}
