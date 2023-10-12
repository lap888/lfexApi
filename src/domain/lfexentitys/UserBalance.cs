using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserBalance
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public decimal BalanceNormal { get; set; }
        public decimal BalanceLock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
