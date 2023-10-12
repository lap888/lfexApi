using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using application;
using domain.entitys;
using domain.models;
using domain.models.lfexDto;
using domain.repository;
using Microsoft.AspNetCore.Mvc;

namespace webAdmin.Controllers
{
    public class RechargeController : Controller
    {
        private readonly IRechargeService RechargeSub;
        public RechargeController(IRechargeService recharge)
        {
            RechargeSub = recharge;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 充值列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<RechargeOrder>>> List([FromBody] QueryRechargeOrder query)
        {
            return await RechargeSub.RechargeOrder(query);
        }

        /// <summary>
        /// 重新查询充值状态
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Query([FromBody] RechargeOrder query)
        {
            return await RechargeSub.QueryOrder(query.OrderNo);
        }

        /// <summary>
        /// 退回糖果 和 果皮
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Refund([FromBody] RechargeOrder query)
        {
            return await RechargeSub.Refund(query);
        }

        /// <summary>
        /// 人工处理
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Succeed([FromBody] RechargeOrder query)
        {
            return await RechargeSub.Succeed(query);
        }


    }
}