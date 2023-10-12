using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace domain.enums
{
    /// <summary>
    /// 账户变更类型
    /// </summary>
    public enum EquityModifyType
    {
        /// <summary>
        /// 股权认购 {份数}
        /// </summary>
        [Description("股权认购 {0}份")]
        EQUITY_SUBSCRIBE = 1,

        /// <summary>
        /// 股权转出 {份数} - {接收人手机号}
        /// </summary>
        [Description("股权转出 {0}份 至 {1}")]
        EQUITY_ROLLOUT = 2,

        /// <summary>
        /// 股权转入 {份数}
        /// </summary>
        [Description("股权转入 {0}份")]
        EQUITY_TRANSFER_INTO = 3,

        /// <summary>
        /// 收购排行 奖励 {榜一} {份数}
        /// </summary>
        [Description("收购达人{0}奖励：{1}份")]
        BUY_CANDY_INTO = 4,
    }
}
