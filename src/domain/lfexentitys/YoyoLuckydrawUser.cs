using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoLuckydrawUser
    {
        public string Id { get; set; }
        public int RoundId { get; set; }
        public long UserId { get; set; }
        public int? AddressId { get; set; }
        public int CandyCount { get; set; }
        public bool IsWin { get; set; }
        public int? PrizeId { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
