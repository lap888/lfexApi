using CSRedis;
using domain.repository;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using Yoyo.Core;

namespace yoyoApi.Controllers
{
    /// <summary>
    /// 升级APP
    /// </summary>
    [ApiController]
    [Route("api/System/ClientDownloadUrl")]
    public class UpdateAppController : ControllerBase
    {
        public ISystemService SystemService { get; set; }
        private readonly IMemoryCache MemoryCache;
        private readonly CSRedisClient RedisCache;
        private readonly bool UseRedis = true;
        private readonly int CacheTime = 1 * 60 * 60;
        public UpdateAppController(ISystemService systemService, IMemoryCache memory, CSRedisClient redisClient)
        {
            SystemService = systemService;
            this.MemoryCache = memory;
            RedisCache = redisClient;
        }

        /// <summary>
        /// 获取系统下载版本
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> ClientDownloadUrl(string name)
        {
            var key = $"System:ClientUrl_{name}";
            if (UseRedis)
            {
                try
                {
                    if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
                    var cacheResult = SystemService.ClientDownloadUrl(name);
                    var cacheString = cacheResult.ToJson(false, true, true);
                    this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
                    return cacheResult;
                }
                catch (Exception ex)
                {
                    LogUtil<UpdateAppController>.Error(ex, "REDIS缓存错误");
                    return SystemService.ClientDownloadUrl(name);
                }
            }
            if (this.MemoryCache.TryGetValue(key, out MyResult<object> result))
            {
                return result;
            }
            result = SystemService.ClientDownloadUrl(name);
            this.MemoryCache.Set(key, result, System.TimeSpan.FromSeconds(CacheTime));
            return result;
        }
    }
}