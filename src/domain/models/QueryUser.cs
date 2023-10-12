using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 查询会员
    /// </summary>
    public class QueryUser : QueryModel
    {
        /// <summary>
        /// 支付宝
        /// </summary>
        public String Alipay { get; set; }

        /// <summary>
        /// 上级手机号
        /// </summary>
        public String InviterMobile { get; set; }

        /// <summary>
        /// 会员状态
        /// </summary>
        public UserState Status { get; set; } = UserState.All;

        /// <summary>
        /// 认证状态
        /// </summary>
        public UserAuthState AuditState { get; set; } = UserAuthState.All;

    }
}
