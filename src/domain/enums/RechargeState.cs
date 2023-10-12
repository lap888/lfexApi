using System;
using System.Collections.Generic;
using System.Text;

namespace domain.enums
{
    /// <summary>
    /// 充值状态
    /// </summary>
    public enum RechargeState
    {
        /// <summary>
        /// 未知
        /// </summary>
        UNKNOWN = 0,

        /// <summary>
        /// 成功
        /// </summary>
        SUCCESS = 1000,

        /// <summary>
        /// 处理中
        /// </summary>
        PROCESSING = 1001,

        /// <summary>
        /// 失败
        /// </summary>
        FAILED = 1005,

        /// <summary>
        /// 未处理
        /// </summary>
        UNTREATED = 1007,

        /// <summary>
        /// 已退款
        /// </summary>
        REFUNDED = 1009,
    }
}
