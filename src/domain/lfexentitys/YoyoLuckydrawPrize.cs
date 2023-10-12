using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoLuckydrawPrize
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Mark { get; set; }
        public string Pic { get; set; }
        public string StatusDesc { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
