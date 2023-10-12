using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models.ticket
{

    /// <summary>
    /// 新人券包
    /// </summary>
    public class TicketPack
    {
        /// <summary>
        /// 份数
        /// </summary>
        public Int32 Shares { get; set; }

        /// <summary>
        /// 糖果价
        /// </summary>
        public Decimal Candy { get; set; }

        /// <summary>
        /// 现金价
        /// </summary>
        public Decimal Cash { get; set; }
    }
}
