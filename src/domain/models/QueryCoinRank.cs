using System;
namespace domain.models
{
    public class QueryCoinRank : QueryModel
    {
        /// <summary>
        /// 0涨幅榜-1跌幅榜-2成交榜
        /// </summary>
        /// <value></value>
        public int Type { get; set; }
        /// <summary>
        /// 0.法币 1.币币
        /// </summary>
        /// <value></value>
        public int CoinType { get; set; }
    }
}
