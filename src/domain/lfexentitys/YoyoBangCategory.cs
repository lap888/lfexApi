using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangCategory
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public decimal MinPrice { get; set; }
        public string Desc { get; set; }
        public int Sort { get; set; }
        public long Pid { get; set; }
        public int IsDel { get; set; }
    }
}
