using System;
using System.Collections.Generic;
using System.Text;

namespace domain.enums
{
    /// <summary>
    /// 奖品类型
    /// </summary>
    public enum ActivityPrizeType
    {
        /// <summary>
        /// 空奖
        /// </summary>
        None = 0,

        /// <summary>
        /// 糖果
        /// </summary>
        Candy = 1,

        /// <summary>
        /// 果皮
        /// </summary>
        Peel = 2,

        /// <summary>
        /// 任务
        /// </summary>
        Task = 3,

        /// <summary>
        /// 实物
        /// </summary>
        Stuff = 4,

        /// <summary>
        /// 券
        /// </summary>
        Coupon = 5,
    }
}
