using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBoxWiner
    {
        public long Id { get; set; }
        public int Period { get; set; }
        public long RecordId { get; set; }
        public long UserId { get; set; }
        public decimal Award { get; set; }
        public decimal Dividend { get; set; }
        public decimal SingleValue { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
