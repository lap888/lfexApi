using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 糖果记录
    /// </summary>
    public class QueryCandyRecord : QueryModel
    {
        /// <summary>
        /// 类型
        /// </summary>
        public Int32 Source { get; set; }
    }
}
