using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GameSuppliers
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public sbyte? IsEnable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
