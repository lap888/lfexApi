﻿using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserAccountWallet
    {
        public long AccountId { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }
        public string CoinType { get; set; }
        public decimal Revenue { get; set; }
        public decimal Expenses { get; set; }
        public decimal Balance { get; set; }
        public decimal Frozen { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
