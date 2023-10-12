namespace domain.models.lfexDto
{
    public class MoveCoinToSomeoneModel
    {
        /// <summary>
        /// 平台渠道名
        /// </summary>
        /// <value></value>
        public string Channel { get; set; }
        /// <summary>
        /// 币种名称
        /// </summary>
        /// <value></value>
        public string CoinName { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        /// <value></value>
        public string Remark { get; set; }

        /// <summary>
        /// 提币数量
        /// </summary>
        /// <value></value>
        public decimal CoinAmount { get; set; }

        /// <summary>
        /// 提币地址
        /// </summary>
        /// <value></value>
        public string Adress { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 消息编号
        /// </summary>
        public string MsgId { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string VerifyCode { get; set; }
        /// <summary>
        /// 交易密码
        /// </summary>
        /// <value></value>
        public string Password { get; set; }
    }

    public class SomeMoveCoinToMeModel : SignModel
    {
        /// <summary>
        /// 转币数量
        /// </summary>
        /// <value></value>
        public decimal CoinAmount { get; set; }
        /// <summary>
        /// 第三方转入用户ID
        /// </summary>
        /// <value></value>
        public string OutUserUuid { get; set; }

        /// <summary>
        /// 转入小鱼指定地址
        /// </summary>
        /// <value></value>
        public string Adress { get; set; }
        /// <summary>
        /// 三方订单ID
        /// </summary>
        /// <value></value>
        public string RefId { get; set; }

        /// <summary>
        /// 币名称
        /// </summary>
        /// <value></value>
        public string CoinName { get; set; }


    }


}