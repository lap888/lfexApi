using domain.enums;
using System;
using System.Collections.Generic;

namespace domain.models
{
    /// <summary>
    /// 活动奖品
    /// </summary>
    public class ActivityPrize
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public long PrizeId { get; set; }
        public string PrizeTitle { get; set; }
        public string Figure { get; set; }
        public string PrizeDesc { get; set; }
        public ActivityPrizeType PrizeType { get; set; }
        public DateTime? WinningTime { get; set; }
        public decimal Bonus { get; set; }
        public bool AutoDeal { get; set; }
        public string Remark { get; set; }
    }
}
