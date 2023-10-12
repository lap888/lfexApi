namespace domain.models.lfexDto
{
    public class FindOrderModel : SignModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        /// <value></value>
        public string OrderNum { get; set; }
        /// <summary>
        /// 渠道号
        /// </summary>
        /// <value></value>
        public string Channel { get; set; }
    }
}