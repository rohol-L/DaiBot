using System.Text.RegularExpressions;

namespace DaiBot.Core.FilterAttributes
{
    public class RegexFilterAttribute : FilterAttribute
    {
        readonly string[] regs;

        public RegexFilterAttribute(params string[] regs)
        {
            this.regs = regs;
        }

        public override bool Check(MessageContext context)
        {
            foreach (var item in regs)
            {
                var reg = new Regex(item);
                if (reg.IsMatch(context.Message))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
