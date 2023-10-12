using System;
using System.Collections.Generic;

namespace domain.models
{
    public class ActivityRaffleEcord
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public long UserId { get; set; }
        public decimal UseCandy { get; set; }
        public decimal UsePeel { get; set; }
        public int IsWin { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
