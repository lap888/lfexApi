using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserBalanceFlow
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Tag { get; set; }
        public string Description { get; set; }
        public decimal? AmountChange { get; set; }
        public int RefId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public sbyte Status { get; set; }
    }
}
