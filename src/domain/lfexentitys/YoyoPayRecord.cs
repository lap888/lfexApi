using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoPayRecord
    {
        public long PayId { get; set; }
        public long UserId { get; set; }
        public int Channel { get; set; }
        public int Currency { get; set; }
        public decimal Amount { get; set; }
        public decimal Fee { get; set; }
        public int ActionType { get; set; }
        public string Custom { get; set; }
        public int PayStatus { get; set; }
        public string ChannelUid { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
    }
}
