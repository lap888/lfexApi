using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoCityMaster
    {
        public int CityId { get; set; }
        public long UserId { get; set; }
        public string CityCode { get; set; }
        public string CityName { get; set; }
        public string WeChat { get; set; }
        public string Mobile { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
