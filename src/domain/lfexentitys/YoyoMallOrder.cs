using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMallOrder
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UnionNo { get; set; }
        public int UnionType { get; set; }
        public string UnionPid { get; set; }
        public string UnionCustom { get; set; }
        public long GoodsId { get; set; }
        public string GoodsName { get; set; }
        public string GoodsImage { get; set; }
        public decimal GoodsPrice { get; set; }
        public int GoodsQuantity { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal Commission { get; set; }
        public int OrderStatus { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ModifyTime { get; set; }
        public string Remark { get; set; }
    }
}
