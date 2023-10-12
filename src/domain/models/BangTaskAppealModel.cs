using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务申诉模型
    /// </summary>
    public class BangTaskAppealModel
    {
        /// <summary>
        /// 申诉编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 任务编号
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 记录编号
        /// </summary>
        public Int64 RecordId { get; set; }

        /// <summary>
        /// 申诉原因
        /// </summary>
        public String AppealReason { get; set; }

        /// <summary>
        /// 申诉图
        /// </summary>
        public String AppealPic { get; set; }
    }
}
