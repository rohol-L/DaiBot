

namespace DaiBot.Core.FilterAttributes
{
    /// <summary>
    /// 仅当没有处理程序被执行时才会访问
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DefaultFilterAttribute : FilterAttribute
    {
        public override bool IsSpecialAttribute => true;
    }
}
