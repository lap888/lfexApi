using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace yoyoApi.Models
{
    /// <summary>
    /// 账户模型
    /// </summary>
    public class AccountModel
    {
        /// <summary>
        /// 总金额
        /// </summary>
        public Decimal TotalAmount { get; set; }

        /// <summary>
        /// 可用金额
        /// </summary>
        public Decimal AvailableAmount { get; set; }

        /// <summary>
        /// 冻结金额
        /// </summary>
        public Decimal FreezeAmount { get; set; }

        /// <summary>
        /// 总支出
        /// </summary>
        public Decimal TotalOutlay { get; set; }

        /// <summary>
        /// 总收入
        /// </summary>
        public Decimal TotalIncome { get; set; }
    }
}
