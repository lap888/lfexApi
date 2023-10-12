using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class CoinTradeLocation
    {
        public string TradeId { get; set; }
        public decimal BuyerLocationX { get; set; }
        public decimal BuyerLocationY { get; set; }
        public string BuyerProvince { get; set; }
        public string BuyerCity { get; set; }
        public string BuyerCityCode { get; set; }
        public string BuyerArea { get; set; }
        public string BuyerAreaCode { get; set; }
        public decimal SellLocationX { get; set; }
        public decimal SellLocationY { get; set; }
        public string SellProvince { get; set; }
        public string SellCity { get; set; }
        public string SellCityCode { get; set; }
        public string SellArea { get; set; }
        public string SellAreaCode { get; set; }
    }
}
