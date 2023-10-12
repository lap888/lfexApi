using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMemberDevote
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public DateTime DevoteDate { get; set; }
        public decimal Devote { get; set; }
    }
}
