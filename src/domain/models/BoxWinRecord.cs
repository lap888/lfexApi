using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    public class BoxWinRecord
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
        /// 奖金
        /// </summary>
        public Decimal Award { get; set; }
        /// <summary>
        /// 分红
        /// </summary>
        public Decimal Dividend { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
