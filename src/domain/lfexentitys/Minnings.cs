using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class Minnings
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public int MinningId { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public sbyte? Status { get; set; }
        public int MinningStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? WorkingTime { get; set; }

        public DateTime? WorkingEndTime { get; set; }
        public int? Source { get; set; }
    }
}
