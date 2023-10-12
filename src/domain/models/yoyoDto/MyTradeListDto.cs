namespace domain.models.yoyoDto
{
    public class MyTradeListDto:BaseModel
    {
        /// <summary>
        /// 买卖
        /// </summary>
        public string Sale { get; set; }
        /// <summary>
        /// 1 我的买单
        /// </summary>
        /// <value></value>
        public int Status { get; set; }
        /// <summary>
        /// 币种名称
        /// </summary>
        /// <value></value>
        public string CoinType { get; set; }
        /// <summary>
        /// fabi bibi
        /// </summary>
        /// <value></value>
        public string Title { get; set; }
    }
}