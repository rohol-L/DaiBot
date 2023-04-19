using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Interface
{
    public interface IMiddleware
    {
        string Name { get; }

        /// <summary>
        /// 排序：
        /// 0-：高优先级
        /// 1~10000：默认
        /// 10000+:低优先级
        /// </summary>
        int Order { get; }

        void Invoke(MessageContext context, Action next);
    }
}
