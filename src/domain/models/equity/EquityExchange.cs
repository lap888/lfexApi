using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models.equity
{
    /// <summary>
    /// 股权兑换
    /// </summary>
    public class EquityExchange
    {
        /// <summary>
        /// 转让人编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 支付密码
        /// </summary>
        public String PayPwd { get; set; }

        /// <summary>
        /// 份数
        /// </summary>
        public Int32 Shares { get; set; }
    }
}
