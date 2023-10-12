using System;

namespace domain.models.yoyoDto
{
    public class CoinTradeExt
    {
        /// <summary>
        /// 可流通糖果
        /// </summary>
        public decimal CanTradeCandy { get; set; } = 0;
        public decimal LastAvgPrice { get; set; } = 0;
        public decimal LastMaxPrice { get; set; } = 0;
        public decimal SysMaxPrice { get; set; } = 2;
        public decimal SysMinPrice { get; set; } = 1;
        /// <summary>
        /// 今日价格
        /// </summary>
        /// <value></value>
        public decimal TodayAmount { get; set; } = 0;
        public decimal TodayAvgPrice { get; set; } = 0;
        public decimal TodayMaxPrice { get; set; } = 0;
        /// <summary>
        /// 今日交易量
        /// </summary>
        /// <value></value>
        public int TodayTradeAmount { get; set; } = 0;
        public int LastTradeAmount { get; set; } = 0;
        /// <summary>
        /// 涨幅
        /// </summary>
        /// <value></value>
        public decimal UpRate { get; set; } = 0.01M;
        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMaxPrice { get; set; }

        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMinPrice { get; set; }


    }
}