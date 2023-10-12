using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public class BangTaskQueryModel : QueryModel
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public Int64 TaskId { get; set; }

        /// <summary>
        /// 搜索关键字
        /// </summary>
        public String Keyword { get; set; }

        /// <summary>
        /// 发布人编号
        /// </summary>
        public Int64 Publisher { get; set; }

        /// <summary>
        /// 分类编号
        /// </summary>
        public Int64 CateId { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public TaskSort Sort { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskState TaskState { get; set; }

        /// <summary>
        /// 奖励类型
        /// </summary>
        public TaskRewardType RewardType { get; set; }
    }
}
