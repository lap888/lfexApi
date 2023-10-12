using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GameCategories
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
