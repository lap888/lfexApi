using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class UserVcodes
    {
        public uint Id { get; set; }
        public string Mobile { get; set; }
        public string MsgId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
