using System;
using System.Collections.Generic;
using System.Text;

namespace application.Models
{
    /// <summary>
    /// 哟帮配置
    /// </summary>
    public class YoBangConfig
    {
        /// <summary>
        /// 任务手续费
        /// </summary>
        public decimal TaskRate { get; set; }

        /// <summary>
        /// 任务上限
        /// </summary>
        public Int32 MaxTask { get; set; }

        /// <summary>
        /// 图片块地址
        /// </summary>
        public String CosUrl { get; set; }

    }
}
