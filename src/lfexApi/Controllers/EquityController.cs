// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CSRedis;
// using domain.entitys;
// using domain.models;
// using domain.models.equity;
// using domain.repository;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using yoyoApi.Controllers.Base;

// namespace yoyoApi.Controllers
// {
//     [ApiController]
//     [Produces("application/json")]
//     [Route("api/[controller]/[action]")]
//     public class EquityController : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly IEquityService EquitySub;
//         public EquityController(CSRedisClient redisClient, IEquityService equity)
//         {
//             RedisCache = redisClient;
//             EquitySub = equity;
//         }

//         /// <summary>
//         /// 股权信息
//         /// </summary>
//         /// <returns></returns>
//         [HttpGet]
//         public async Task<MyResult<EquityInfo>> EquityPage()
//         {
//             return await EquitySub.EquityPage(base.TokenModel.Id);
//         }

//         /// <summary>
//         /// 股权兑换
//         /// </summary>
//         /// <param name="exchange"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<object>> ExchangeEquity([FromBody] EquityExchange exchange)
//         {
//             exchange.UserId = base.TokenModel.Id;
//             return await EquitySub.ExchangeEquity(exchange);
//         }


//         /// <summary>
//         /// 股权转让
//         /// </summary>
//         /// <param name="transfer"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<object>> TransferEquity([FromBody]EquityTransfer transfer)
//         {
//             transfer.UserId = base.TokenModel.Id;
//             return await EquitySub.TransferEquity(transfer);
//         }

//         /// <summary>
//         /// 股权记录
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<UserAccountEquityRecord>>> EquityRecords([FromBody]QueryModel query)
//         {
//             query.UserId = base.TokenModel.Id;
//             return await EquitySub.EquityRecords(query);
//         }

//     }
// }