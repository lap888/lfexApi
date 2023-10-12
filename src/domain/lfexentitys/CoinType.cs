using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CoinType
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public decimal Fee { get; set; }
        public int Count24 { get; set; }
        public int CountTotal { get; set; }
        public int MinCanMove { get; set; }
        public decimal NowPrice { get; set; }
        public decimal LastPrice { get; set; }
        public decimal UpDown { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Remark { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
