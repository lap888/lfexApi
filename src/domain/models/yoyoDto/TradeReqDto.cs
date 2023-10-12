using System;

namespace domain.models.yoyoDto
{
    public class TradeReqDto : BaseModel
    {
        /// <summary>
        /// 
        /// </summary>
        public String Sale { get; set; }
        /// <summary>
        /// type 0 amount type price
        /// </summary>
        /// <value></value>
        public string Type { get; set; } = "amount";
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public string Order { get; set; } = "desc";
        /// <summary>
        /// 币种
        /// </summary>
        /// <value></value>
        public string CoinType { get; set; }
        public string SearchText { get; set; } = "";
    }
}