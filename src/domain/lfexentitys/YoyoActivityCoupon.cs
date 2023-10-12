using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoActivityCoupon
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long WinId { get; set; }
        public int CouponType { get; set; }
        public DateTime EffectiveTime { get; set; }
        public DateTime? UseTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
