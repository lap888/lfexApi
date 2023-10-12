using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models.equity
{
    /// <summary>
    /// 股权信息
    /// </summary>
    public class EquityInfo
    {
        /// <summary>
        /// 真实名称
        /// </summary>
        public String TrueName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public String IDCardNum { get; set; }

        /// <summary>
        /// 糖果数
        /// </summary>
        public Decimal Candy { get; set; }

        /// <summary>
        /// 果皮数
        /// </summary>
        public Decimal Peel { get; set; }

        /// <summary>
        /// 糖果限制
        /// </summary>
        public Decimal CandyLimit { get; set; }

        /// <summary>
        /// 任务限制
        /// </summary>
        public String TaskLimit { get; set; }

        /// <summary>
        /// 认购单价
        /// </summary>
        public Decimal UnitPrice { get; set; }

        /// <summary>
        /// 总份数
        /// </summary>
        public Int32 TotalShares { get; set; }

        /// <summary>
        /// 可转
        /// </summary>
        public Int32 Convertible { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public String Rules { get; set; }
    }
}
