using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GemRecords
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public decimal? Num { get; set; }
        public DateTime? GemMinningAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Description { get; set; }
        public int? GemSource { get; set; }
    }
}
