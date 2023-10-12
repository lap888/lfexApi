using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 糖果分红模型
    /// </summary>
    public class CandyDividendModel
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
        /// 类型
        /// </summary>
        public Int32 DividendSource { get; set; }
        /// <summary>
        /// 分红描述
        /// </summary>
        public String DividendDesc { get; set; }
    }
}
