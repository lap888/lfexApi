using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBoxRecord
    {
        public long Id { get; set; }
        public int Period { get; set; }
        public long UserId { get; set; }
        public decimal UnitPrice { get; set; }
        public int BuyTotal { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
