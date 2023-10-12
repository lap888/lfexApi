using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yoyoApi.Controllers.Enums
{
    /// <summary>
    /// 
    /// </summary>
    public enum BangTaskState
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 0,

        /// <summary>
        /// 待审核
        /// </summary>
        WaitAudit = 1,

        /// <summary>
        /// 已通过
        /// </summary>
        Normal = 2,

        /// <summary>
        /// 已拒绝
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 4,

        /// <summary>
        /// 已取消
        /// </summary>
        Cancelled = 5,

        /// <summary>
        /// 已关闭
        /// </summary>
        Closed = 6,
    }
}
