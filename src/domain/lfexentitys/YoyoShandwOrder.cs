using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoShandwOrder
    {
        public long Id { get; set; }
        public string ChannelNo { get; set; }
        public string ChannelUid { get; set; }
        public string ChannelOrderNo { get; set; }
        public string GameAppId { get; set; }
        public string Product { get; set; }
        public long UserId { get; set; }
        public decimal PayMoney { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PayTime { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
