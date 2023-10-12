using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangRank
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public int TaskType { get; set; }
        public decimal OfferPrice { get; set; }
        public DateTime EffectiveTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
