using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMemberActive
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string JpushId { get; set; }
        public DateTime ActiveTime { get; set; }
        public string Remark { get; set; }
    }
}
