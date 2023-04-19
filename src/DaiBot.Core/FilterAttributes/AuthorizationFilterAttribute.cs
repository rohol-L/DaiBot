namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 只允许包含某个权限时访问
    /// </summary>
    public class AuthorizationFilterAttribute : FilterAttribute
    {
        readonly string[] args;

        public AuthorizationFilterAttribute(params string[] args)
        {
            this.args = args;
        }

        public override bool Check(MessageContext context)
        {
            foreach (var arg in args)
            {
                if (context.Authorization.Contains(arg))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
