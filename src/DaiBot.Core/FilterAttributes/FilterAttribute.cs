using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.FilterAttributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class FilterAttribute : Attribute
    {
        public virtual bool IsSpecialAttribute => false;

        /// <summary>
        /// 支持自定义参数，会自动调用参数个数最多的Check函数
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual bool Check(MessageContext context)
        {
            return true;
        }
    }
}
