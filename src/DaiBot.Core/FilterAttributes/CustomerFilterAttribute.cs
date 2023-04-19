

namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 自定义过滤
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CustomerFilterAttribute : FilterAttribute
    {
        public override bool IsSpecialAttribute => true;
    }
}
