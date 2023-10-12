using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    public class BaseTask
    {
        /// <summary>
        /// 任务ID，可用于排序
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public int TaskType { get; set; }
        /// <summary>
        /// 任务标题
        /// </summary>
        public string TaskTitle { get; set; }
        /// <summary>
        /// 任务描述
        /// </summary>
        public string TaskDesc { get; set; }
        /// <summary>
        /// 任务奖励
        /// </summary>
        public decimal Reward { get; set; }
        /// <summary>
        /// 任务目标
        /// </summary>
        public int Aims { get; set; }
        /// <summary>
        /// 单位
        /// </summary>
        public string Unit { get; set; }
        /// <summary>
        /// 任务已完成进度
        /// </summary>
        public int Carry { get; set; }
        /// <summary>
        /// 任务状态,1:已完成，0:未完成
        /// </summary>
        public int Completed { get; set; }
    }
}
