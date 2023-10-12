using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 记录模型
    /// </summary>
    public class RecordModel
    {
        /// <summary>
        /// 记录编号
        /// </summary>
        public Int64 RecordId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public String Title { get; set; }

        /// <summary>
        /// 发生金额
        /// </summary>
        public Decimal OccurAmount { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public String Desc { get; set; }

        /// <summary>
        /// 发生时间
        /// </summary>
        public DateTime OccurTime { get; set; }

    }
}
