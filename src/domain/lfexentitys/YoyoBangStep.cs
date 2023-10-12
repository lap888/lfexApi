using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangStep
    {
        public long Id { get; set; }
        public long TaskId { get; set; }
        public int Type { get; set; }
        public string Describe { get; set; }
        public string Content { get; set; }
        public int Sort { get; set; }
        public string Remark { get; set; }
    }
}
