using domain.enums;
using System;
using System.Collections.Generic;

namespace domain.models
{
    /// <summary>
    /// 中奖记录
    /// </summary>
    public class ActivityWinRecord
    {
        public long Id { get; set; }
        public string ActivityTitle { get; set; }
        public string PrizeTitle { get; set; }
        public string Figure { get; set; }
        public string PrizeDesc { get; set; }
        public ActivityPrizeType PrizeType { get; set; }
        public decimal Bonus { get; set; }
        public string Nick { get; set; }
        public string HeadImg { get; set; }
        public string Mobile { get; set; }
        public DateTime WinningTime { get; set; }
        public string Contact { get; set; }
        public string Person { get; set; }
        public string Postcode { get; set; }
        public string MailingAddress { get; set; }
        public ActivityReceiveState State { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public string Remark { get; set; }
    }
}
