using application.Utils;
using CSRedis;
using domain.models;
using domain.models.yoyoDto;
using domain.repository;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Yoyo.Core;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    [Route("api/[controller]/[action]")]
    public class SystemController : ApiBaseController
    {
        public ISystemService SystemService { get; set; }
        private readonly CSRedisClient RedisCache;
        private readonly bool UseRedis = true;
        private readonly int CacheTime = 1 * 60 * 60;
        public SystemController(ISystemService systemService, IMemoryCache memory, CSRedisClient redisClient)
        {
            SystemService = systemService;
            RedisCache = redisClient;
        }
        /// <summary>
        /// 推荐排行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> Ranking([FromQuery] GetRankingRequest request)
        {
            MyResult<object> rult = new MyResult<object>()
            {
                Data = await SystemService.Recommend(request)
            };
            return rult;
        }

        /// <summary>
        /// 分享排行榜
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> ShareRanking(int type)
        {
            MyResult<object> rult = new MyResult<object>()
            {
                Data = await SystemService.ShareRank(type)
            };
            return rult;
        }

        /// <summary>
        /// 求购排行
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> DuplicateRanking(int type)
        {
            MyResult<object> rult = new MyResult<object>()
            {
                Data = await SystemService.Duplicate(type)
            };
            return rult;
        }
        /// <summary>
        /// 获取轮播图
        /// </summary>
        /// <param name="source">0 首页轮播 1 游戏页面轮播 2 广告</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> Banners(int source)
        {
            Int64 UserId = base.TokenModel.Id;
            if (UserId > 0 && source == 2)
            {
                return SystemService.Banners(source, UserId);
            }
            var key = $"System:Banners_{source}";
            if (UseRedis)
            {
                try
                {
                    if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                    var cacheResult = SystemService.Banners(source);
                    var cacheString = cacheResult.ToJson(false, true, true);
                    this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                    return cacheResult;
                }
                catch (Exception ex)
                {
                    LogUtil<SystemController>.Error(ex, "REDIS缓存错误");
                    return SystemService.Banners(source);
                }
            }
            MyResult<object> result = SystemService.Banners(source);
            return result;

        }


        /// <summary>
        /// 消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public MyResult<object> Notices([FromBody] NoticesDto model)
        {
            if (model.Type != 1)
            {
                var key = $"System:Notices_{model.Type}_{model.PageIndex}_{model.PageSize}";
                if (UseRedis)
                {
                    try
                    {
                        if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                        var cacheResult = SystemService.Notices(model, base.TokenModel.Id);
                        var cacheString = cacheResult.ToJson(false, true, true);
                        this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                        return cacheResult;
                    }
                    catch (Exception ex)
                    {
                        LogUtil<SystemController>.Error(ex, "REDIS缓存错误");
                        return SystemService.Notices(model, base.TokenModel.Id);
                    }
                }
                MyResult<object> result = SystemService.Notices(model, base.TokenModel.Id);
                return result;
            }

            return SystemService.Notices(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 获取公告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> OneNotice()
        {
            var key = "System:OneNotice";
            if (UseRedis)
            {
                try
                {
                    if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                    var cacheResult = SystemService.OneNotice();
                    var cacheString = cacheResult.ToJson(false, true, true);
                    this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                    return cacheResult;
                }
                catch (Exception ex)
                {
                    LogUtil<SystemController>.Error(ex, "REDIS缓存错误");
                    return SystemService.OneNotice();
                }
            }
            MyResult<object> result = SystemService.OneNotice();
            return result;
        }

        /// <summary>
        /// 任务商店
        /// </summary>
        /// <param name="status">0 商店在售任务  1 过期任务</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> TasksShop(int status)
        {
            return await SystemService.TasksShop(base.TokenModel.Id, status);
        }
        /// <summary>
        /// 获取用户关键信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<UserReturnDto> InitInfo()
        {
            return SystemService.UserInfo(base.TokenModel.Id);
        }
        /// <summary>
        /// 兑换任务
        /// </summary>
        /// <param name="minningId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Exchange(int minningId)
        {
            return await SystemService.Exchange(minningId, base.TokenModel.Id);
        }

        /// <summary>
        /// 获取APP文案
        /// </summary>
        /// <param name="type">文案类型</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<MyResult<object>> CopyWriting(string type)
        {
            return await SystemService.CopyWriting(type);
        }
    }
}