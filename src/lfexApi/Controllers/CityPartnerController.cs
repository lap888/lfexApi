// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CSRedis;
// using domain.models;
// using domain.repository;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using yoyoApi.Controllers.Base;

// namespace yoyoApi.Controllers
// {
//     [ApiController]
//     [Produces("application/json")]
//     [Route("api/city/[action]")]
//     public class CityPartnerController : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly ICityPartnerService CityPartner;
//         public CityPartnerController(CSRedisClient redisClient, ICityPartnerService cityPartner)
//         {
//             RedisCache = redisClient;
//             CityPartner = cityPartner;
//         }

//         /// <summary>
//         /// 获取城市信息
//         /// </summary>
//         /// <param name="code"></param>
//         /// <returns></returns>
//         [HttpGet]
//         public async Task<MyResult<CityInfoModel>> Info(String code)
//         {
//             MyResult<CityInfoModel> result = new MyResult<CityInfoModel>();
//             result.Data = await CityPartner.CityInfo(code, base.TokenModel.Id);
//             return result;
//         }

//         /// <summary>
//         /// 分红记录
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<DividendRecord>>> Record([FromBody] QueryCityRecord query)
//         {
//             query.UserId = base.TokenModel.Id;
//             return await CityPartner.CityRecord(query);
//         }

//         /// <summary>
//         /// 设置联系方式
//         /// </summary>
//         /// <param name="contact"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> SetContact([FromBody] ContactModel contact)
//         {
//             contact.UserId = base.TokenModel.Id;
//             return await CityPartner.SetContact(contact);
//         }

//     }
// }