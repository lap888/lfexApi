using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CityEarnings
    {
        public string CityNo { get; set; }
        public decimal Cash { get; set; }
        public decimal Candy { get; set; }
        public int People { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
