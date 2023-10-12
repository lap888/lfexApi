using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserCandyp
    {
        public uint Id { get; set; }
        public int UserId { get; set; }
        public decimal CandyP { get; set; }
        public string Content { get; set; }
        public short Source { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
