using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaiBot.Core.Interface
{
    /// <summary>
    /// 所有插件需要实现此接口
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 插件版本
        /// </summary>
        public string Version { get; }

        /// <summary>
        /// 插件载入时自动调用
        /// </summary>
        public void OnLoad();

        /// <summary>
        /// 插件卸载时自动调用
        /// </summary>
        public void OnUnload();
    }
}
