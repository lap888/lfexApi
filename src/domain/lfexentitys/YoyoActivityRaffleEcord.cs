using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoActivityRaffleEcord
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public long PrizeId { get; set; }
        public long UserId { get; set; }
        public decimal UseCandy { get; set; }
        public decimal UsePeel { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
