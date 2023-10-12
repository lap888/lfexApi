using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class Relations
    {
        public int Id { get; set; }
        public string Mobile { get; set; }
        public string InviterMobile { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
