using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoBangTask
    {
        public long Id { get; set; }
        public long Publisher { get; set; }
        public string Project { get; set; }
        public string Title { get; set; }
        public long CateId { get; set; }
        public string Desc { get; set; }
        public int SubmitHour { get; set; }
        public int Step { get; set; }
        public int AuditHour { get; set; }
        public int IsRepeat { get; set; }
        public int RewardType { get; set; }
        public decimal UnitPrice { get; set; }
        public int Complete { get; set; }
        public decimal FeeRate { get; set; }
        public int Total { get; set; }
        public int State { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string Remark { get; set; }
    }
}
