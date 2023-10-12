using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class Pictures
    {
        public long Id { get; set; }
        public string Url { get; set; }
        public string ImageableType { get; set; }
        public int? ImageableId { get; set; }
        public int? Size { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Type { get; set; }
    }
}
