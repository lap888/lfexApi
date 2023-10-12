// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using CSRedis;
// using domain.enums;
// using domain.models;
// using domain.models.yoyoDto;
// using domain.repository;
// using infrastructure.utils;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Yoyo.Core;
// using yoyoApi.Controllers.Base;
// using yoyoApi.Models;

// namespace yoyoApi.Controllers
// {
//     [ApiController]
//     [Produces("application/json")]
//     [Route("api/[controller]/[action]")]
//     public class AccountController : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly IUserWalletAccountService WalletAccount;
//         private readonly ICandyService CandySub;
//         public AccountController(CSRedisClient redisClient, IUserWalletAccountService userWallet, ICandyService candyService)
//         {
//             RedisCache = redisClient;
//             CandySub = candyService;
//             WalletAccount = userWallet;
//         }

//         /// <summary>
//         /// 钱包账户信息
//         /// </summary>
//         /// <returns></returns>
//         [HttpGet]
//         public async Task<MyResult<AccountModel>> WallerInfo()
//         {
//             MyResult<AccountModel> result = new MyResult<AccountModel>();
//             var rult = await WalletAccount.WalletAccountInfo(this.TokenModel.Id);
//             if (!rult.Success) { return result.SetStatus(ErrorCode.InvalidData, rult.Message); }

//             var accInfo = rult.Data;
//             result.Data = new AccountModel()
//             {
//                 TotalAmount = accInfo.Balance,
//                 AvailableAmount = accInfo.Balance - accInfo.Frozen,
//                 FreezeAmount = accInfo.Frozen,
//                 TotalIncome = accInfo.Revenue,
//                 TotalOutlay = accInfo.Expenses
//             };

//             return result;
//         }

//         /// <summary>
//         /// 钱包账户记录
//         /// </summary>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<UserAccountWalletRecord>>> WallerRecord([FromBody] QueryWalletRecord query)
//         {
//             MyResult<List<UserAccountWalletRecord>> result = new MyResult<List<UserAccountWalletRecord>>();
//             query.CurrentId = this.TokenModel.Id;
//             var rult = await WalletAccount.WalletAccountRecord(query.CurrentId, query.PageIndex, query.PageSize, query.ModifyType);
//             if (!rult.Success) { return result.SetStatus(ErrorCode.InvalidData, rult.Message); }
//             result.RecordCount = rult.RecordCount;
//             result.PageCount = rult.PageCount;
//             result.Data = new List<UserAccountWalletRecord>();
//             foreach (var item in rult.Data)
//             {
//                 item.AccountId = 0;
//                 var a1 = item.ModifyType.GetDescription();
//                 var a2 = item.ModifyDesc.Split(",");
//                 item.ModifyDesc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(","));
//                 result.Data.Add(item);
//             }
//             return result;
//         }

//         /// <summary>
//         /// 钱包提现
//         /// </summary>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<object>> Withdraw([FromBody] WithdrawModel withdraw)
//         {
//             String TradeNo = Gen.NewGuid20();
//             return await WalletAccount.Withdraw(base.TokenModel.Id, withdraw.Amount, withdraw.TradePwd, TradeNo);
//         }

//         /// <summary>
//         /// 糖果记录
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<RecordModel>>> CandyRecord([FromBody] QueryCandyRecord query)
//         {
//             query.UserId = base.TokenModel.Id;
//             return await CandySub.CandyRecord(query);
//         }

//     }
// }