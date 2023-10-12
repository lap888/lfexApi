using domain.models.ticket;
using System;
using System.Collections.Generic;
using System.Text;

namespace application.Models
{
    /// <summary>
    /// 新人券配置
    /// </summary>
    public class TicketConfig
    {
        /// <summary>
        /// 认购总数
        /// </summary>
        public Int32 SubscribeTotal { get; set; }

        /// <summary>
        /// 每日可兑换份数
        /// </summary>
        public Int32 DayShares { get; set; }

        /// <summary>
        /// 任务次数
        /// </summary>
        public Int32 TaskCount { get; set; }

        /// <summary>
        /// 套餐包
        /// </summary>
        public List<TicketPack> Package { get; set; }

        /// <summary>
        /// 规则
        /// </summary>
        public String Rules { get; set; }
    }

}
