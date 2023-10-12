using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoActivityPrize
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public string PrizeTitle { get; set; }
        public string Figure { get; set; }
        public string PrizeDesc { get; set; }
        public int PrizeType { get; set; }
        public int AutoDeal { get; set; }
        public decimal Bonus { get; set; }
        public int Quantity { get; set; }
        public int WinRatio { get; set; }
        public int DailyWins { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
