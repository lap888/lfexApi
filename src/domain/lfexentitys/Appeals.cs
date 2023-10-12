using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class Appeals
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string PicUrl { get; set; }
        public string Description { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public sbyte? AppealResult { get; set; }
    }
}
