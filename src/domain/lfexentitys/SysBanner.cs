using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class SysBanner
    {
        public uint Id { get; set; }
        public sbyte Queue { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public sbyte Type { get; set; }
        public sbyte Source { get; set; }
        public sbyte Status { get; set; }
        public string Params { get; set; }
        public string CityCode { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
