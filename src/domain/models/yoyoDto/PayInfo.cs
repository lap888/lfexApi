using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace domain.models.yoyoDto
{
    public class PayInfo
    {
        public long PayId { get; set; }

        public long UserId { get; set; }

        public PayChannel Channel { get; set; }

        public Currency Currency { get; set; }

        public decimal Amount { get; set; }

        public decimal Fee { get; set; }

        public ActionType ActionType { get; set; }

        public string Custom { get; set; }

        public PayStatus PayStatus { get; set; }

        public string ChannelUID { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? ModifyTime { get; set; }
    }

    public enum PayChannel
    {
        YoPay = 1,
        AliPay = 2,
        WePay = 3
    }

    public enum Currency
    {
        Candy = 0,
        Rmb = 1
    }

    public enum ActionType
    {
        [Description("修改支付宝")]
        CHANGE_ALIPAY = 0,
        [Description("支付宝二次认证")]
        AUTH_ALIPAY = 1,
        [Description("付款至用户")]
        TRANSFER_TO_USER = 2,
        [Description("钱包现金充值")]
        CASH_RECHARGE = 3,
        [Description("钱包提现")]
        CASH_WITH_DRAW = 4
    }

    public enum PayStatus
    {
        INVALID = -1,
        UN_PAID = 0,
        PAID = 1,
        REFUND = 2
    }
}
