using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GoldFlows
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Discribe { get; set; }
        public string Num { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public sbyte? IsRead { get; set; }
    }
}
