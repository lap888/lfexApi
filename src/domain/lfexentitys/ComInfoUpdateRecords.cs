using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class ComInfoUpdateRecords
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public int Type { get; set; }
        public DateTime? Udate { get; set; }
    }
}
