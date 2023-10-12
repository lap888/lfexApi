using System;

namespace domain.models
{
    public class LookUpIncomeModel
    {
        public string Id { get; set; }
        public long UserId { get; set; }
        public int Type { get; set; }

        public int Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CTime { get; set; }
        public DateTime SopTime { get; set; }
    }
}