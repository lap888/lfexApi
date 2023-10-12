using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 发布任务
    /// </summary>
    public class BangTaskInfoModel
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public long TaskId { get; set; }

        /// <summary>
        /// 券编号
        /// </summary>
        public long CouponId { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string UserPic { get; set; }
        /// <summary>
        /// 发布人
        /// </summary>
        public long Publisher { get; set; }
        /// <summary>
        /// 项目名
        /// </summary>
        public string Project { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 分类编号
        /// </summary>
        public long CateId { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 做单时长
        /// </summary>
        public int SubmitHour { get; set; }
        /// <summary>
        /// 审核时长
        /// </summary>
        public int AuditHour { get; set; }
        /// <summary>
        /// 重复类型
        /// </summary>
        public TaskRepeatType IsRepeat { get; set; }
        /// <summary>
        /// 奖励类型
        /// </summary>
        public TaskRewardType RewardType { get; set; }
        /// <summary>
        /// 任务单价
        /// </summary>
        public decimal UnitPrice { get; set; }
        /// <summary>
        /// 任务总数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 完成数
        /// </summary>
        public int FinishCount { get; set; }

        /// <summary>
        /// 剩余数
        /// </summary>
        public int RemainderCount { get; set; }

        /// <summary>
        /// 步骤
        /// </summary>
        public List<BangTaskStepModel> Steps { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        public Decimal FeeRate { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskState State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 是否已报名
        /// </summary>
        public int IsDoTaskState { get; set; }
    }
}
