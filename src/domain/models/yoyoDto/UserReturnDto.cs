namespace domain.models.yoyoDto
{
    public class UserReturnDto
    {
        /// <summary>
        /// 提币失败原因
        /// </summary>
        /// <value></value>
        public string FailReason { get; set; }
        /// <summary>
        /// 糖果总产量
        /// </summary>
        /// <value></value>
        public decimal CandyNum { get; set; } = 0;
        /// <summary>
        /// 果皮
        /// </summary>
        /// <value></value>
        public decimal CandyP { get; set; } = 0;
        /// <summary>
        /// 果核
        /// </summary>
        /// <value></value>
        public decimal CandyH { get; set; } = 0;
        /// <summary>
        /// 钱包余额
        /// </summary>
        /// <value></value>
        public decimal UserBalanceNormal { get; set; }
        /// <summary>
        /// 冻结余额
        /// </summary>
        public decimal UserBalanceLock { get; set; }
        /// <summary>
        /// 日产量
        /// </summary>
        /// <value></value>
        public decimal DayNum { get; set; } = 0;
        /// <summary>
        /// 是否做今日任务
        /// </summary>
        /// <value></value>
        public int IsDoTask { get; set; } = 0;
        /// <summary>
        /// 手机号
        /// </summary>
        /// <value></value>
        public string Mobile { get; set; }

        /// <summary>
        /// 观看广告次数
        /// </summary>
        public int TotalWatch { get; set; }

        /// <summary>
        /// 认证广告次数
        /// </summary>
        public int AuthAdCount { get; set; }

        public string Level { get; set; }
        public int Golds { get; set; }
        public string Rcode { get; set; }
        public string inviterMobile { get; set; }

        public int Status { get; set; }
        public int AuditState { get; set; }

        public int IsPay { get; set; }

        public string Alipay { get; set; }

        public string AlipayPic { get; set; }

        public string AlipayUid { get; set; }

        /// <summary>
        /// 我的联系电话
        /// </summary>
        public string MyContactTel { get; set; }
        /// <summary>
        /// 我的微信号
        /// </summary>
        public string MyWeChatNo { get; set; }
        /// <summary>
        /// 上级联系电话
        /// </summary>
        public string ReContactTel { get; set; }

        /// <summary>
        /// 上级微信号
        /// </summary>
        public string ReWeChatNo { get; set; }

        /// <summary>
        /// 任务进度
        /// </summary>
        public decimal TaskSchedule { get; set; }

        /// <summary>
        /// 任务开始时间
        /// </summary>
        public string TaskStartTime { get; set; }

        /// <summary>
        /// 任务结束时间
        /// </summary>
        public string TaskEndTime { get; set; }

        /// <summary>
        /// 交易刷新间隔
        /// </summary>
        public int AdInterval { get; set; }

        /// <summary>
        /// 交易上拉次数
        /// </summary>
        public int PullUpTimes { get; set; }
        /// <summary>
        /// 热钱包地址
        /// </summary>
        /// <value></value>
        public string Adress { get; set; }
    }
}