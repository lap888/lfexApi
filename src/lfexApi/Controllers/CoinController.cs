using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSRedis;
using domain.enums;
using domain.lfexentitys;
using domain.models;
using domain.models.lfexDto;
using domain.repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yoyo.Core;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class CoinController : ApiBaseController
    {
        private readonly CSRedisClient RedisCache;
        private readonly IUserWalletAccountService WalletAccount;
        private readonly ICoinService CoinService;
        public CoinController(CSRedisClient redisClient, IUserWalletAccountService userWallet, ICoinService coinService)
        {
            RedisCache = redisClient;
            CoinService = coinService;
            WalletAccount = userWallet;
        }

        /// <summary>
        /// 获取币排行榜FindCoinRank
        /// 1涨跌榜-2新币榜-3成交榜
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<List<CoinTypeModel>>> FindCoinRank([FromBody] QueryCoinRank model)
        {
            return await CoinService.FindCoinRank(model);
        }

        /// <summary>
        /// 获取币种
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<List<CoinTypeModel>>> FindCoinType()
        {
            return await CoinService.FindCoinType(this.TokenModel.Id);
        }

        /// <summary>
        /// 获取账户币资产
        /// </summary>
        /// <param name="type">0 币币 1法币</param>
        /// <returns></returns>
        [HttpGet]
        // [AllowAnonymous]
        public async Task<MyResult<CoinUserAccountWallet>> FindCoinAmount(int type)
        {
            var userId = base.TokenModel.Id;
            return await CoinService.FindCoinAmount(type, userId);
        }

        /// <summary>
        /// 各币种明细记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<UserAccountWalletRecord>>> CoinRecord([FromBody] QueryWalletRecord query)
        {
            MyResult<List<UserAccountWalletRecord>> result = new MyResult<List<UserAccountWalletRecord>>();
            query.UserId = this.TokenModel.Id;
            var rult = await CoinService.CoinAccountRecord(query.CurrentId, query.UserId, query.PageIndex, query.PageSize, query.ModifyType);
            if (!rult.Success) { return result.SetStatus(ErrorCode.InvalidData, rult.Message); }
            result.RecordCount = rult.RecordCount;
            result.PageCount = rult.PageCount;
            result.Data = new List<UserAccountWalletRecord>();
            foreach (var item in rult.Data)
            {
                item.AccountId = 0;
                item.ModifyDesc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(","));
                result.Data.Add(item);
            }
            return result;
        }
        /// <summary>
        /// 第三方查询订单需要授权
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<CoinMoveRecord>> FindOrder([FromBody] FindOrderModel model)
        {
            return await CoinService.FindOrder(model);
        }

        /// <summary>
        /// 第三方检查区块链地址
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> CheckAdress([FromBody] CheckAdressModel model)
        {
            return await CoinService.CheckAdress(model);
        }
        /// <summary>
        /// 第三方平台转币个给小鱼[服务端对接使用]
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> SomeoneMoveCoinTome(SomeMoveCoinToMeModel model)
        {
            return await CoinService.SomeoneMoveCoinTome(model);
        }

        /// <summary>
        /// 小鱼交易所提币到其他平台
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> MoveCoinToSomeone(MoveCoinToSomeoneModel model)
        {
            return await CoinService.MoveCoinToSomeone(model, this.TokenModel.Id);
        }

        /// <summary>
        /// 提币发送验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> MoveCoinSendCode(string mobile)
        {
            return await CoinService.MoveCoinSendCode(mobile, this.TokenModel.Id);
        }
        /// <summary>
        /// 矿机列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<List<MinningDto>>> MinningList()
        {
            return await CoinService.MinningList(this.TokenModel.Id);
        }
        /// <summary>
        /// 挖矿接口
        /// </summary>
        /// <param name="mId">矿机列表返回ID</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> DoTask(int mId)
        {
            return await CoinService.DoTask(this.TokenModel.Id, mId);
        }
        /// <summary>
        /// 修复矿机
        /// </summary>
        /// <param name="mId">矿机序列ID</param>
        /// <returns></returns>
        [HttpGet]
        // [AllowAnonymous]
        public async Task<MyResult<object>> RepairMinning(int mId)
        {
            return await CoinService.RepairMinning(this.TokenModel.Id, mId);
            // return await CoinService.RepairMinning(1, mId);
        }
        /// <summary>
        /// 贡献值流水
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> GlodsRecord(BaseModel model)
        {
            return CoinService.GlodsRecord(model, this.TokenModel.Id);
        }
        /// <summary>
        /// 锁仓收益配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> LookUpIncomeSetting()
        {
            return await CoinService.LookUpIncomeSetting();
        }

        /// <summary>
        /// 确认锁仓
        /// </summary>
        /// <param name="type">锁仓类型</param>
        /// <param name="amount">锁仓LF</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> ConfirmLookUp(int type, decimal amount)
        {
            return await CoinService.ConfirmLookUp(this.TokenModel.Id, type, amount);
        }

        /// <summary>
        /// 我的锁仓订单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> MinnersOrder(LooKUpMinnerModel model)
        {
            return await CoinService.MinnersOrder(model, this.TokenModel.Id);
        }

        /// <summary>
        /// 赎回
        /// </summary>
        /// <param name="orderId">赎回订单号</param>
        /// <returns></returns>
        [HttpGet]
        // [AllowAnonymous]
        public async Task<MyResult<object>> SopOrder(string orderId)
        {
            return await CoinService.SopOrder(this.TokenModel.Id, orderId);
            // return await CoinService.SopOrder(2093, orderId);
        }

        /// <summary>
        ///  K 线 数据 0 分时 1 15分钟 2 30分钟 3 1小时 4 1天 5 1个月
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> KLine([FromBody] QueryKLine model)
        {
            return await CoinService.KLine(model);
        }
        /// <summary>
        /// 面板数据
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> KLinePanel(string coinType)
        {
            return await CoinService.KLinePanel(coinType);
        }
        /// <summary>
        /// type =0 委托订单 type =1 最新成交  type=2 币种简介
        /// </summary>
        /// <param name="type"></param>
        /// <param name="coinType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> CoinData(int type, string coinType)
        {
            return await CoinService.CoinData(type, coinType);
        }

        /// <summary>
        /// 交易NewData
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> NewCoinData(string coinType)
        {
            return await CoinService.NewCoinData(coinType);
        }

    }
}
