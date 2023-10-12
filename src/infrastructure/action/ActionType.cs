using System.ComponentModel;

namespace infrastructure.action
{
    /// <summary>
    /// 父菜单类型-自定义系统模块
    /// </summary>
    public enum ActionType
    {
        [Description("系统管理")]
        SystemManager = 1,
        [Description("用户管理")]
        UsersManager,
        [Description("财务管理")]
        CaiwuManager,
        [Description("市场设置")]
        ShiChangManager,
        [Description("LFEX服务")]
        YoyoService,
        [Description("订单管理")]
        OrderManager,
        [Description("会员管理")]
        UserManager,
        [Description("Yo帮管理")]
        YoBangManager,
        [Description("夺宝管理")]
        LuckyDrawManager,
        [Description("充值管理")]
        RechargeManager,
        [Description("数据分析")]
        DataAsync

    }
}