using System;
using domain.enums;

namespace domain.models.lfexDto
{
    public partial class ActivityCouponDto
    {
        /// <summary>
        /// 编号
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 会员编号
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 中奖编号
        /// </summary>
        public string WinId { get; set; }
        /// <summary>
        /// 券类型
        /// </summary>
        public CouponType CouponType { get; set; }
        /// <summary>
        /// 生效时间
        /// </summary>
        public DateTime EffectiveTime { get; set; }
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public ActivityCouponState State { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }
    }
}