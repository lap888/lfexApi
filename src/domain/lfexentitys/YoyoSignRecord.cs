using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoSignRecord
    {
        public string Sign { get; set; }
        public DateTime ServerTime { get; set; }
        public DateTime ClientTime { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
    }
}
