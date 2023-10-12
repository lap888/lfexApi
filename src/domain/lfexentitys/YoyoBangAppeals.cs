using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangAppeals
    {
        public long Id { get; set; }
        public long RecordId { get; set; }
        public long TaskId { get; set; }
        public string Pictures { get; set; }
        public string Reason { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
