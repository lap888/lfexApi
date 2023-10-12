using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoActivity
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Figure { get; set; }
        public string Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan? StartLotteryTime { get; set; }
        public TimeSpan? EndLotteryTime { get; set; }
        public decimal UseCandy { get; set; }
        public decimal UsePeel { get; set; }
        public int DailyLimit { get; set; }
        public int ActivityLimit { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
