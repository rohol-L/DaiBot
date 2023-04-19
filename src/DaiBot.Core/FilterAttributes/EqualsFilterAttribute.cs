namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 值完全匹配时访问
    /// </summary>
    public class EqualsFilterAttribute : FilterAttribute
    {
        readonly string[] keywords;

        public EqualsFilterAttribute(params string[] keywords)
        {
            this.keywords = keywords;
        }

        public override bool Check(MessageContext context)
        {
            foreach (var item in keywords)
            {
                if (context.Message.Trim().Equals(item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
