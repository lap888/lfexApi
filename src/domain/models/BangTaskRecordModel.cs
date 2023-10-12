using domain.enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务记录列表
    /// </summary>
    public class BangTaskRecordModel
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
        /// 任务标题
        /// </summary>
        public String TaskTitle { get; set; }

        /// <summary>
        /// 赏金
        /// </summary>
        public Decimal UnitPrice { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 会员昵称
        /// </summary>
        public String UserNick { get; set; }

        /// <summary>
        /// 会员头像
        /// </summary>
        public String UserPic { get; set; }

        /// <summary>
        /// 任务详情
        /// </summary>
        public String TaskDetail { private get; set; }

        /// <summary>
        /// 任务详情
        /// </summary>
        public List<BangTaskStepModel> TaskSteps
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<List<BangTaskStepModel>>(TaskDetail);
                }
                catch { return new List<BangTaskStepModel>(); }
            }            
        }

        /// <summary>
        /// 报名时间
        /// </summary>
        public DateTime EntryTime { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime? SubmitTime { get; set; }

        /// <summary>
        /// 截止时间
        /// </summary>
        public DateTime? CutoffTime { get; set; }

        /// <summary>
        /// 审核时间
        /// </summary>
        public DateTime? AuditTime { get; set; }

        /// <summary>
        /// 审核截止时间
        /// </summary>
        public DateTime? AuditCutoffTime { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskRecordState RecordState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public String Remark { get; set; } = String.Empty;
    }
}
