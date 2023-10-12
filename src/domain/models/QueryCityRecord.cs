using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 分红记录
    /// </summary>
    public class QueryCityRecord : QueryModel
    {
        /// <summary>
        /// 城市编号
        /// </summary>
        public String CityNo { get; set; }

        /// <summary>
        /// 分红类型
        /// </summary>
        public DividendType DividendType { get; set; }

        /// <summary>
        /// 账户类型
        /// </summary>
        public Int32 AccountType { get; set; }
    }
}
