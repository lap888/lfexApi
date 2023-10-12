using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;



namespace domain.lfexentitys
{
    public partial class SystemRoles
    {
        [NotMapped]
        public virtual List<string> Menus { get; set; }
    }

    public partial class UserAccountEquityRecord
    {
        public enums.LfexCoinnModifyType ModifyType { get; set; }
    }
    
    public partial class UserAccountTicketRecord
    {
        public enums.LfexCoinnModifyType ModifyType { get; set; }
    }

    public partial class UserAccountWalletRecord
    {
        public enums.LfexCoinnModifyType ModifyType { get; set; }
    }
    
}