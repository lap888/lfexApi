using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using application;
using CSRedis;
using domain.enums;
using domain.lfexentitys;
using domain.models;
using domain.models.admin;
using domain.models.lfexDto;
using domain.models.yoyoDto;
using domain.repository;
using Microsoft.AspNetCore.Mvc;

namespace webAdmin.Controllers
{
    /// <summary>
    /// 会员管理
    /// </summary>
    public class MemberController : Base.WebBaseController
    {
        private readonly IRealVerify RealVerify;
        private readonly IQCloudPlugin QCloudSub;
        private readonly CSRedisClient RedisCache;
        private readonly IYoyoUserSerivce YoyoUserSerivce;
        private readonly ITradeService UserWalletAccountService;

        public MemberController(ITradeService userWalletAccountService, IYoyoUserSerivce yoyoUserSerivce, IRealVerify realVerify, IQCloudPlugin qCloud, CSRedisClient redisClient)
        {
            YoyoUserSerivce = yoyoUserSerivce;
            RealVerify = realVerify;
            QCloudSub = qCloud;
            RedisCache = redisClient;
            UserWalletAccountService = userWalletAccountService;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 用户币种钱包
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> WalletlList(int id)
        {
            return await YoyoUserSerivce.WalletlList(id);
        }
        /// <summary>
        /// 钱包币种明细
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="accountId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> WalletlRecordList([FromBody] WalletRcordsDto model)
        {
            return await YoyoUserSerivce.WalletlRecordList(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Datasync([FromBody] WalletRcordsDto model)
        {
            return await YoyoUserSerivce.Datasync(model);
        }

        /// <summary>
        /// 会员列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<ListModel<AdminUserModel>>> List([FromBody] QueryUser query)
        {
            MyResult<ListModel<AdminUserModel>> result = new MyResult<ListModel<AdminUserModel>>();

            result.Data = await YoyoUserSerivce.UserList(query);
            result.RecordCount = result.Data.Total;
            result.PageCount = result.Data.Total / query.PageSize;

            return result;
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Freeze([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await YoyoUserSerivce.Freeze(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "解冻失败"); }
            return Rult;
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Unfreeze([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await YoyoUserSerivce.Unfreeze(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "解冻失败"); }
            return Rult;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Modify([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await YoyoUserSerivce.Modify(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "修改失败"); }
            return Rult;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> ModifyUsdt([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            if (user.Count < 0)
            {
                return Rult.SetStatus(ErrorCode.InvalidData, "修改失败");
            }
            if (string.IsNullOrWhiteSpace(user.CoinType))
            {
                return Rult.SetStatus(ErrorCode.InvalidData, "修改失败");
            }
            var money = user.Types == 0 ? user.Count : -user.Count;
            var flat = user.Types == 0 ? LfexCoinnModifyType.System_Add : LfexCoinnModifyType.System_Sub;
            var re = await UserWalletAccountService.ChangeWalletAmount(null, false, user.Id, user.CoinType, money, flat, false);
            if (re.Code != 200)
            {
                return Rult.SetStatus(ErrorCode.InvalidData, re.Message);
            }
            return Rult;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> AuthInfo([FromBody] UserDto user)
        {
            MyResult<object> Rult = new MyResult<object>();
            Rult.Data = await YoyoUserSerivce.AuthInfo(user);
            if (Rult.Data == null) { Rult.SetStatus(ErrorCode.InvalidData, "认证信息不存在"); }
            return Rult;
        }



    }
}