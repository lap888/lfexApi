using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserAccountEquityRecord
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        // public enums.LfexCoinnModifyType ModifyType { get; set; }
        public string ModifyDesc { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
