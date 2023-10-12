using System;
using System.Collections.Generic;
using System.Text;

namespace application.Models
{
    /// <summary>
    /// 通用配置
    /// </summary>
    public class AppSetting
    {
        /// <summary>
        /// 支付宝异步通知地址
        /// </summary>
        public String AlipayNotify { get; set; }

        /// <summary>
        /// 提现费率
        /// </summary>
        public Decimal WithdrawRate { get; set; }

        /// <summary>
        /// 交易开始时间
        /// </summary>
        public DateTime TradeStartTime { get; set; }

        /// <summary>
        /// 交易结束时间
        /// </summary>
        public DateTime TradeEndTime { get; set; }

        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMaxPrice { get; set; }

        /// <summary>
        /// 卖单最大单价
        /// </summary>
        public Decimal SellMinPrice { get; set; }

        /// <summary>
        /// 交易提示
        /// </summary>
        public String TradeTips { get; set; }

        /// <summary>
        /// 限制任务编号
        /// </summary>
        public List<Int32> TaskLimitIds { get; set; }

        /// <summary>
        /// 限时任务 限制
        /// </summary>
        public Int32 TaskLimit { get; set; }

        /// <summary>
        /// 任务加速 分钟数
        /// </summary>
        public Int32 TaskQuicken { get; set; }

        /// <summary>
        /// 解封交易 需要的糖果数
        /// </summary>
        public Decimal TradeUnblockCandy { get; set; }

        /// <summary>
        /// 解封交易 需要的果皮数
        /// </summary>
        public Decimal TradeUnblockPeel { get; set; }

        /// <summary>
        /// 修改支付宝 需要的糖果数
        /// </summary>
        public Decimal AlipayModifyCandy { get; set; }

        /// <summary>
        /// 修改支付宝 需要的果皮数
        /// </summary>
        public Decimal AlipayModifyPeel { get; set; }

        /// <summary>
        /// 认证广告次数
        /// </summary>
        public Int32 AuthAdCount { get; set; }

        /// <summary>
        /// 用户等级配置
        /// </summary>
        public List<UserLevel> Levels { get; set; }

        /// <summary>
        /// 交易刷新间隔
        /// </summary>
        public int AdInterval { get; set; }

        /// <summary>
        /// 交易上拉次数
        /// </summary>
        public int PullUpTimes { get; set; }
    }
}
