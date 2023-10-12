using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务查询
    /// </summary>
    public class QuerySysTaskModel : QueryModel
    {
        /// <summary>
        /// 记录编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 任务编号
        /// </summary>
        public Int32 TaskId { get; set; } = -1;

        /// <summary>
        /// 上级手机号
        /// </summary>
        public String InviterMobile { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public Int32 Status { get; set; } = -1;

        /// <summary>
        /// 来源
        /// </summary>
        public Int32 Source { get; set; } = -1;
    }
}
