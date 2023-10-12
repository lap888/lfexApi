using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CoinTradeExt
    {
        public uint Id { get; set; }
        public string Type { get; set; }
        public double SysMaxPrice { get; set; }
        public double SysMinPrice { get; set; }
        public double TodayAmount { get; set; }
        public double TodayAvgPrice { get; set; }
        public double TodayMaxPrice { get; set; }
        public int TodayTradeAmount { get; set; }
        public double UpRate { get; set; }
        public DateTime Ctime { get; set; }
    }
}
