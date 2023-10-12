using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 活动信息
    /// </summary>
    public class BoxActivityInfo
    {
        /// <summary>
        /// 期次
        /// </summary>
        public Int32 Period { get; set; }
        /// <summary>
        /// 奖池金额
        /// </summary>
        public Decimal PrizePool { get; set; }
        /// <summary>
        /// 当前单价
        /// </summary>
        public Decimal UnitPrice { get; set; }
        /// <summary>
        /// 购买总量
        /// </summary>
        public Int32 BuyTotal { get; set; }
        /// <summary>
        /// 上期奖金
        /// </summary>
        public Decimal PreAward { get; set; }
        /// <summary>
        /// 上期分红
        /// </summary>
        public Decimal PreDividend { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
