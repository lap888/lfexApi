using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBoxActivity
    {
        public long Id { get; set; }
        public int Period { get; set; }
        public decimal PrizePool { get; set; }
        public decimal UnitPrice { get; set; }
        public int BuyTotal { get; set; }
        public DateTime EndTime { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
