using domain.models;
using domain.lfexentitys;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using domain.models.lfexDto;

namespace domain.repository
{
    /// <summary>
    /// 充值
    /// </summary>
    public interface IRechargeService
    {
        /// <summary>
        /// 获取手机号信息
        /// </summary>
        /// <param name="Phone"></param>
        /// <returns></returns>
        Task<MyResult<PhoneInfoModel>> MobileInfo(String Phone);

        /// <summary>
        /// 手机号充值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> MobileRecharge(PhoneRechargeModel model);

        /// <summary>
        /// 充值订单查询
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        Task<MyResult<Object>> QueryOrder(String OrderNo);

        /// <summary>
        /// 我的充值订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<RechargeOrder>>> RechargeOrder(QueryRechargeOrder query);

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Refund(RechargeOrder order);

        /// <summary>
        /// 人工处理
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<Object>> Succeed(RechargeOrder order);
    }
}
