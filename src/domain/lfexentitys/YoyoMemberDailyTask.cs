using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMemberDailyTask
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int TaskId { get; set; }
        public int Carry { get; set; }
        public DateTime CompleteDate { get; set; }
        public int Completed { get; set; }
    }
}
