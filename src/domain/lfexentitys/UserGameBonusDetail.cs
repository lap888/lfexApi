using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserGameBonusDetail
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int OrderId { get; set; }
        public decimal BonusAmount { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
