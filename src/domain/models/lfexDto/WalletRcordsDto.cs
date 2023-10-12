using System;

namespace domain.models.lfexDto
{
    public class WalletRcordsDto : BaseModel
    {
        public int? UserId { get; set; }
        public int? AccountId { get; set; }

        public string CoinType { get; set; }
        public string Mobile { get; set; }
        public string Channel { get; set; }

        public DateTime? ModifyTime { get; set; }
    }
}