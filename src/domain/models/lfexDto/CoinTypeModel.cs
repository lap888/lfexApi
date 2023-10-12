using System;

namespace domain.models.lfexDto
{
    public class CoinTypeModel
    {
        /// <summary>
        /// 币排名
        /// </summary>
        /// <value></value>
        public int RankId { get; set; }
        /// <summary>
        /// 币种
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// 24小时交易量
        /// </summary>
        /// <value></value>
        public int Count24 { get; set; }
        /// <summary>
        /// 发行量
        /// </summary>
        /// <value></value>
        public int CountTotal { get; set; }
        /// <summary>
        /// 当前价格
        /// </summary>
        /// <value></value>
        public decimal NowPrice { get; set; }
        /// <summary>
        /// 昨日开盘价
        /// </summary>
        /// <value></value>
        public decimal LastPrice { get; set; }
        /// <summary>
        /// 涨跌幅
        /// </summary>
        /// <value></value>
        public decimal UpDown { get; set; }
        /// <summary>
        /// 币状态0未上线 1上线
        /// </summary>
        /// <value></value>
        public int Status { get; set; }
        /// <summary>
        /// 冲币币状态0未开放 1开放
        /// </summary>
        /// <value></value>
        public int Cstatus { get; set; }
        /// <summary>
        /// 币种类 0 法币 1 币币
        /// </summary>
        /// <value></value>
        public int Type { get; set; }
        /// <summary>
        /// 币说明
        /// </summary>
        /// <value></value>
        public string Remark { get; set; }
        /// <summary>
        /// 上架交易所时间
        /// </summary>
        /// <value></value>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 手续费
        /// </summary>
        /// <value></value>
        public decimal Fee { get; set; }
        
        /// <summary>
        /// 最小提币数量
        /// </summary>
        /// <value></value>
        public int MinCanMove { get; set; }

        /// <summary>
        /// 可用余额
        /// </summary>
        /// <value></value>
        public decimal Balance { get; set; }
    }
}