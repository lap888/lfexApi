using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CityCashDividendRecord
    {
        public long Id { get; set; }
        public string CityNo { get; set; }
        public int ModifyType { get; set; }
        public string ModifyDesc { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        public DateTime? CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
