using System;
using System.Collections.Generic;

namespace domain.models.lfexDto
{
    public class UserAccountWalletModel
    {
        public long AccountId { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }
        public string CoinType { get; set; }
        /// <summary>
        /// 总收入
        /// </summary>
        /// <value></value>
        public decimal Revenue { get; set; }
        /// <summary>
        /// 总支出
        /// </summary>
        /// <value></value>
        public decimal Expenses { get; set; }
        /// <summary>
        /// 余额
        /// </summary>
        /// <value></value>
        public decimal Balance { get; set; }
        /// <summary>
        /// 冻结
        /// </summary>
        /// <value></value>
        public decimal Frozen { get; set; }
        /// <summary>
        /// USDT 折合价
        /// </summary>
        /// <value></value>
        public decimal UsPrice { get; set; }
        
        public DateTime ModifyTime { get; set; }
    }

    public class CoinUserAccountWallet
    {
        /// <summary>
        /// 美元折合总价
        /// </summary>
        /// <value></value>
        public decimal DPriceTotal { get; set; } = 0;
        /// <summary>
        /// 人民币折合总价
        /// </summary>
        /// <value></value>
        public decimal RPriceTotal { get; set; } = 0;

        public List<UserAccountWalletModel> Lists = new List<UserAccountWalletModel>();
    }
}