﻿using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class SystemRolePermission
    {
        public int RoleId { get; set; }
        public string ActionId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
