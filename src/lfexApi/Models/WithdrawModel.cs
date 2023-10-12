using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yoyoApi.Models
{
    /// <summary>
    /// 提现模型
    /// </summary>
    public class WithdrawModel
    {
        /// <summary>
        /// 提现金额
        /// </summary>
        public Decimal Amount { get; set; }

        /// <summary>
        /// 交易密码
        /// </summary>
        public String TradePwd { get; set; }
    }
}
