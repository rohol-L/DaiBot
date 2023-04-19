namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 开头匹配到时访问
    /// </summary>
    public class StartWithFilterAttribute : FilterAttribute
    {
        readonly string[] keywords;

        public StartWithFilterAttribute(params string[] keywords)
        {
            this.keywords = keywords;
        }

        public override bool Check(MessageContext context)
        {
            foreach (var item in keywords)
            {
                if (context.Message.StartsWith(item))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
