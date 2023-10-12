using System;
namespace domain.models
{
    public class QueryKLine : BaseModel
    {
        /// <summary>
        /// 0 分时 1 15分钟 2 30分钟 3 1小时 4 1天 5 1个月
        /// </summary>
        /// <value></value>
        public int Type { get; set; }

        /// <summary>
        /// eg:糖果，YB，LF
        /// </summary>
        /// <value></value>
        public string CoinType { get; set; }
    }
}
