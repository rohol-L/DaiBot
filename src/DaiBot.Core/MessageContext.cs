using Autofac;
using DaiBot.Core.Delegates;
using System.Collections.Generic;

namespace DaiBot.Core
{
    public class MessageContext
    {
        public string SenderName { get; init; }
        public long UserID { get; set; }
        public string? UserNick { get; set; }
        public long GroupID { get; set; }
        public long MessageID { get; set; }
        public string RawMessage { get; set; }
        public string Message { get; set; }
        public DateTime MessageTime { get; set; } = DateTime.Now;

        public List<string> CommandPayload { get; } = new();

        public HashSet<string> Authorization { get; } = new();

        public object? OtherPayload { get; set; }

        /// <summary>
        /// 定义如何回显消息
        /// </summary>
        public ResponseDelegate Callback { get; set; }

        private ILifetimeScope? scope = null;

        public ILifetimeScope Scope
        {
            get
            {
                if (scope == null)
                {
                    throw new Exception("Scope is null");
                }
                return scope;
            }
            set
            {
                scope = value;
            }
        }

        public MessageContext(string senderName, string msg, string rawMsg, ResponseDelegate callback)
        {
            SenderName = senderName;
            Message = msg;
            RawMessage = rawMsg;
            Callback = callback;
        }
    }
}