using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务步骤
    /// </summary>
    public class BangTaskStepModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 步骤炻
        /// </summary>
        public StepType Type { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Describe { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
    }
}
