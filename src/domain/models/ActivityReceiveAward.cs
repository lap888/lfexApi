using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 领奖
    /// </summary>
    public class ActivityReceiveAward
    {
        public long WinId { get; set; }
        public long UserId { get; set; }
        public string ActivityId { get; set; }
        public long PrizeId { get; set; }
        public string Contact { get; set; }
        public string Person { get; set; }
        public string Postcode { get; set; }
        public string MailingAddress { get; set; }
        public long AddressId { get; set; }
        public string Remark { get; set; }
    }
}
