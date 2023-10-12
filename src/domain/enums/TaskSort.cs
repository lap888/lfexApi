using System;
using System.Collections.Generic;
using System.Text;

namespace domain.enums
{
    /// <summary>
    /// 任务排序
    /// </summary>
    public enum TaskSort
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default = 0,

        /// <summary>
        /// 最新
        /// </summary>
        Newest = 1,

        /// <summary>
        /// 信用值
        /// </summary>
        CreditVal = 2,

        /// <summary>
        /// 价格
        /// </summary>
        Price = 3,

        /// <summary>
        /// 复杂度
        /// </summary>
        complexity = 4
    }
}
