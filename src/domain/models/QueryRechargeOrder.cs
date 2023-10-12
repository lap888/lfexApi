using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    public class QueryRechargeOrder : QueryModel
    {
        /// <summary>
        /// 充值手机号
        /// </summary>
        public String RechargePhone { get; set; }
        /// <summary>
        /// 订单类型
        /// </summary>
        public Int32 OrderType { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public enums.RechargeState State { get; set; }
    }
}
