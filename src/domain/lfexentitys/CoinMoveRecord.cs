using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CoinMoveRecord
    {
        public uint Id { get; set; }
        public string UserId { get; set; }
        public string RefId { get; set; }
        public string Address { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public int Status { get; set; }
        public string CoinType { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
