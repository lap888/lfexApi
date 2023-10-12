using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 城市信息
    /// </summary>
    public class CityInfoModel : ContactModel
    {
        /// <summary>
        /// 头像
        /// </summary>
        public String Avatar { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public String CityName { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public String EffectiveTime { get; set; }

        /// <summary>
        /// 糖果收益
        /// </summary>
        public Decimal CandyEarnings { get; set; } = 0.0000M;

        /// <summary>
        /// 现金收益
        /// </summary>
        public Decimal CashEarnings { get; set; }

        /// <summary>
        /// 城市人数
        /// </summary>
        public Decimal People { get; set; }

        /// <summary>
        /// 拉新分红
        /// </summary>
        public Decimal PullNew { get; set; }

        /// <summary>
        /// 做任务分红
        /// </summary>
        public Decimal TaskCandy { get; set; }

        /// <summary>
        /// 交易分红
        /// </summary>
        public Decimal TransactionCandy { get; set; }

        /// <summary>
        /// 哟帮糖果分红
        /// </summary>
        public Decimal YoBangCandy { get; set; }

        /// <summary>
        /// 哟帮现金分红
        /// </summary>
        public Decimal YoBangCash { get; set; }

        /// <summary>
        /// 糖果分红
        /// </summary>
        public Decimal CandyDividend { get; set; }

        /// <summary>
        /// 游戏分红
        /// </summary>
        public Decimal GameDividend { get; set; }

        /// <summary>
        /// 视频分红
        /// </summary>
        public Decimal VideoDividend { get; set; }

        /// <summary>
        /// 商城分红
        /// </summary>
        public Decimal MallDividend { get; set; }
    }
}
