using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务追加
    /// </summary>
    public class BangTaskAppendModel
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 任务总数
        /// </summary>
        public Int32 TaskTotal { get; set; }

        /// <summary>
        /// 任务单价
        /// </summary>
        public Decimal TaskPrice { get; set; }
    }
}
