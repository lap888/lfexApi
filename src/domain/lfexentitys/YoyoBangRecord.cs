using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangRecord
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TaskId { get; set; }
        public string TaskDetail { get; set; }
        public DateTime EntryTime { get; set; }
        public DateTime? SubmitTime { get; set; }
        public DateTime CutoffTime { get; set; }
        public DateTime? AuditTime { get; set; }
        public int State { get; set; }
        public string Remark { get; set; }
    }
}
