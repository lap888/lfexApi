using System;
using domain.enums;

namespace domain.models.lfexDto
{
    /// <summary>
    /// 充值订单
    /// </summary>
    public partial class RechargeOrder
    {
        /// <summary>
        /// 编号
        /// </summary>
        public Int64 Id { get; set; }

        /// <summary>
        /// 订单号
        /// </summary>
        public String OrderNo { get; set; }

        /// <summary>
        /// 渠道订单号
        /// </summary>
        public String ChannelNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>
        public Int32 OrderType { get; set; }

        /// <summary>
        /// 会员编号
        /// </summary>
        public Int64 UserId { get; set; }

        /// <summary>
        /// 商品编号
        /// </summary>
        public String ProductId { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public String ProductName { get; set; }

        /// <summary>
        /// 面值
        /// </summary>
        public String FaceValue { get; set; }

        /// <summary>
        /// 账户
        /// </summary>
        public String Account { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public Decimal Price { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        public Int32 BuyNum { get; set; }

        /// <summary>
        /// 支付糖果
        /// </summary>
        public Decimal PayCandy { get; set; }

        /// <summary>
        /// 支付果皮
        /// </summary>
        public Decimal PayPeel { get; set; }

        /// <summary>
        /// 进货价
        /// </summary>
        public Decimal PurchasePrice { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public RechargeState State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public String Remark { get; set; }
    }

}