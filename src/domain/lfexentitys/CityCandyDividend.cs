using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CityCandyDividend
    {
        public long Id { get; set; }
        public string CityNo { get; set; }
        public int DividendType { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
