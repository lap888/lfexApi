﻿using domain.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 查询券模型
    /// </summary>
    public class QueryActivityCoupon : QueryModel
    {
        /// <summary>
        /// 券类型
        /// </summary>
        public CouponType Type { get; set; }
        /// <summary>
        /// 券状态
        /// </summary>
        public ActivityCouponState State { get; set; }
    }
}
