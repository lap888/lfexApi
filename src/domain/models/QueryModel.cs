using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 通用查询模型
    /// </summary>
    public class QueryModel
    {
        /// <summary>
        /// 通用编号
        /// </summary>
        public Int64 CurrentId { get; set; } = 0;

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Mobile { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public Int32 PageIndex { get; set; } = 1;

        /// <summary>
        /// 页量
        /// </summary>
        public Int32 PageSize { get; set; } = 10;
    }
}
