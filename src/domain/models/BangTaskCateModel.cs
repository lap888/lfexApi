using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 任务类别
    /// </summary>
    public class BangTaskCateModel
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Desc { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }
        /// <summary>
        /// 父类编号
        /// </summary>
        public long? Pid { get; set; }
        /// <summary>
        /// 下级分类
        /// </summary>
        public List<BangTaskCateModel> Categories { get; set; }
    }
}
