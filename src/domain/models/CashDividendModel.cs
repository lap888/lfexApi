using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 现金分红模型
    /// </summary>
    public class CashDividendModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }
        /// <summary>
        /// 分红金额
        /// </summary>
        public Decimal Amount { get; set; }
        /// <summary>
        /// 分红类型
        /// </summary>
        public AccountModifyType DividendType { get; set; }
        /// <summary>
        /// 分红描述
        /// </summary>
        public string[] DividendDesc { get; set; }
    }
}
