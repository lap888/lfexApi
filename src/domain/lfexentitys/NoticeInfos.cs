﻿using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class NoticeInfos
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int IsRead { get; set; }
        public string RefId { get; set; }
        public string Type { get; set; }
        public DateTime? CeratedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
