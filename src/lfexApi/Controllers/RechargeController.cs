// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using application;
// using application.Request;
// using CSRedis;
// using domain.models;
// using domain.repository;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using yoyoApi.Controllers.Base;

// namespace yoyoApi.Controllers
// {
//     [ApiController]
//     [Produces("application/json")]
//     [Route("api/[controller]/[action]")]
//     public class RechargeController : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly IRechargeService RechargeSub;
//         public RechargeController(CSRedisClient redisClient, IRechargeService recharge)
//         {
//             RedisCache = redisClient;
//             RechargeSub = recharge;
//         }

//         /// <summary>
//         /// 获取手机号信息
//         /// </summary>
//         /// <param name="phone"></param>
//         /// <returns></returns>
//         [HttpGet]
//         public async Task<MyResult<PhoneInfoModel>> QueryMobile(String phone)
//         {
//             return await RechargeSub.MobileInfo(phone);
//         }

//         /// <summary>
//         /// 手机号充值
//         /// </summary>
//         /// <param name="recharge"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> RechargeMobile([FromBody] PhoneRechargeModel recharge)
//         {
//             recharge.UserId = base.TokenModel.Id;
//             return await RechargeSub.MobileRecharge(recharge);
//         }
//     }
// }