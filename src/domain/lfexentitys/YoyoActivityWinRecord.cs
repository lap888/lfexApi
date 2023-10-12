using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoActivityWinRecord
    {
        public long Id { get; set; }
        public long ActivityId { get; set; }
        public long PrizeId { get; set; }
        public long UserId { get; set; }
        public DateTime WinningTime { get; set; }
        public string Person { get; set; }
        public string Contact { get; set; }
        public string Postcode { get; set; }
        public string MailingAddress { get; set; }
        public int State { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public string Remark { get; set; }
    }
}
