using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoLuckydrawRound
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public int PrizeId { get; set; }
        public int? Status { get; set; }
        public int NeedRoundNumber { get; set; }
        public int CurrentRoundNumber { get; set; }
        public int MaxNumber { get; set; }
        public bool AutoNext { get; set; }
        public int WinnerType { get; set; }
        public int DelayHour { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
