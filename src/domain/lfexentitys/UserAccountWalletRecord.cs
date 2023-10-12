﻿using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserAccountWalletRecord
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        // public enums.AccountModifyType ModifyType { get; set; }
        public string ModifyDesc { get; set; }
        public DateTime ModifyTime { get; set; }
    }
}
