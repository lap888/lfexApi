using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models.equity
{
    /// <summary>
    /// 股权记录
    /// </summary>
    public class EquityRecord
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public String Title { get; set; }
        
        /// <summary>
        /// 份数
        /// </summary>
        public Int32 Shares { get; set; }

        /// <summary>
        /// 说明
        /// </summary>
        public String Desc { get; set; }

        /// <summary>
        /// 时间
        /// </summary>
        public DateTime CreateTime { get; set; }
    }
}
