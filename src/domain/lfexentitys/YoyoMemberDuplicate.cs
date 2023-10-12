using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMemberDuplicate
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public long UserId { get; set; }
        public decimal Duplicate { get; set; }
    }
}
