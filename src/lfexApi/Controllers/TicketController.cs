using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSRedis;
using domain.lfexentitys;
using domain.models;
using domain.models.ticket;
using domain.repository;
using Microsoft.AspNetCore.Mvc;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    /// <summary>
    /// 新人券
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class TicketController : ApiBaseController
    {
        private readonly CSRedisClient RedisCache;
        private readonly ITicketService TicketSub;
        public TicketController(CSRedisClient redisClient, ITicketService ticketService)
        {
            RedisCache = redisClient;
            TicketSub = ticketService;
        }

        /// <summary>
        /// 新人券页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<TicketModel>> Info()
        {
            return await TicketSub.TicketPage(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券开关
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Switch()
        {
            return await TicketSub.TicketSwitch(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券任务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Task()
        {
            return await TicketSub.TicketTask(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Exchange([FromBody] TicketExchange exchange)
        {
            exchange.UserId = base.TokenModel.Id;
            return await TicketSub.ExchangeTicket(exchange);
        }

        /// <summary>
        /// 使用新人券
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<Object>> Use()
        {
            return await TicketSub.UseTicket(base.TokenModel.Id);
        }

        /// <summary>
        /// 新人券记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<UserAccountTicketRecord>>> Records(QueryModel query)
        {
            query.UserId = base.TokenModel.Id;
            return await TicketSub.TicketRecords(query);
        }
    }
}