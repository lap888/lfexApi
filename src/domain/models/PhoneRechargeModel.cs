using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 手机号充值
    /// </summary>
    public class PhoneRechargeModel
    {
        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public String Phone { get; set; }

        /// <summary>
        /// 面值
        /// </summary>
        public Decimal FaceValue { get; set; }

        /// <summary>
        /// 支付密码
        /// </summary>
        public String PayPwd { get; set; }
    }
}
