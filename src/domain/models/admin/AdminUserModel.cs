using System;
using System.Text;
using System.Collections.Generic;
using domain.models.yoyoDto;

namespace domain.models.admin
{
    /// <summary>
    /// 会员模型
    /// </summary>
    public class AdminUserModel : UserDto
    {
        /// <summary>
        /// 余额
        /// </summary>
        public Decimal Balance { get; set; }

        /// <summary>
        /// 冻结
        /// </summary>
        public Decimal Frozen { get; set; }

    }
}
