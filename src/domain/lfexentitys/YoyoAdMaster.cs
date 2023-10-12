using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoAdMaster
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public string Place { get; set; }
        public string ImgSrc { get; set; }
        public string Alt { get; set; }
        public string Url { get; set; }
        public int Sort { get; set; }
        public int Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
