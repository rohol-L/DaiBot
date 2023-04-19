
namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 只允许特定命令访问
    /// </summary>
    public class CommandFilterAttribute : FilterAttribute
    {
        readonly string[] args;

        public CommandFilterAttribute(params string[] args)
        {
            this.args = args;
        }

        public override bool Check(MessageContext context)
        {
            foreach (var arg in args)
            {
                if (context.CommandPayload.FirstOrDefault() == arg)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
