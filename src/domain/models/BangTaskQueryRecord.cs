using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务记录
    /// </summary>
    public class BangTaskQueryRecord : QueryModel
    {
        /// <summary>
        /// 记录编号
        /// </summary>
        public Int64 RecordId { get; set; }

        /// <summary>
        /// 任务编号
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 步骤
        /// </summary>
        public List<BangTaskStepModel> Steps { get; set; }

        /// <summary>
        /// 记录状态
        /// </summary>
        public TaskRecordState RecordState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public String Remark { get; set; }

    }


}
