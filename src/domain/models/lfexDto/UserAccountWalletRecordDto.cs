using System;

namespace domain.models.lfexDto
{
    public class UserAccountWalletRecordDto
    {
        public long RecordId { get; set; }
        public long AccountId { get; set; }
        public decimal PreChange { get; set; }
        public decimal Incurred { get; set; }
        public decimal PostChange { get; set; }
        public string ModifyDesc { get; set; }

        public enums.LfexCoinnModifyType ModifyType { get; set; }
        public DateTime ModifyTime { get; set; }
        public string CoinType { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
    }
}