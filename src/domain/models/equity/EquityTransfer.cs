using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models.equity
{
    /// <summary>
    /// 股权转让
    /// </summary>
    public class EquityTransfer
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
        /// 被转让人 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 转让份数
        /// </summary>
        public Int32 Shares { get; set; }
    }
}
