using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 通知模型
    /// </summary>
    public class RechargeNotify
    {
        /// <summary>
        /// 福禄开放平台订单号
        /// </summary>
        [JsonProperty("order_id")]
        public String OrderId { get; set; }

        /// <summary>
        /// 交易完成时间
        /// </summary>
        [JsonProperty("charge_finish_time")]
        public String ChargeFinishTime { get; set; }

        /// <summary>
        /// 合作商家订单号
        /// </summary>
        [JsonProperty("customer_order_no")]
        public String CustomerOrderNo { get; set; }

        /// <summary>
        /// 订单状态
        /// </summary>
        [JsonProperty("order_status")]
        public String OrderStatus { get; set; }

        /// <summary>
        /// 充值描述
        /// </summary>
        [JsonProperty("recharge_description")]
        public String RechargeDescription { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        [JsonProperty("product_id")]
        public String ProductId { get; set; }

        /// <summary>
        /// 交易单价
        /// </summary>
        [JsonProperty("price")]
        public String Price { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        [JsonProperty("buy_num")]
        public String BuyNum { get; set; }

        /// <summary>
        /// 运营商流水号
        /// </summary>
        [JsonProperty("operator_serial_number")]
        public String OperatorSerialNumber { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        [JsonProperty("sign")]
        public String Sign { get; set; }
    }
}
