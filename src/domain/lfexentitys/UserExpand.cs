using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserExpand
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Mobile { get; set; }
        public string Wechat { get; set; }
        public DateTime CreateTime { get; set; }
        public string Remark { get; set; }
    }
}
