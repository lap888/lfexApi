using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    public class BoxBet
    {
        /// <summary>
        /// 期次
        /// </summary>
        public Int32 Period { get; set; }
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 下注数量
        /// </summary>
        public Int32 BetTotal { get; set; }
    }
}
