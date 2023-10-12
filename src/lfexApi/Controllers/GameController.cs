// using CSRedis;
// using domain.enums;
// using domain.models.yoyoDto;
// using domain.repository;
// using infrastructure.extensions;
// using infrastructure.utils;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Caching.Memory;
// using System;
// using Yoyo.Core;

// namespace yoyoApi.Controllers
// {
//     [Route("api/[controller]/[action]")]
//     public class GameController : Base.ApiBaseController
//     {
//         public IGameService GameService { get; set; }
//         private readonly IMemoryCache MemoryCache;
//         private readonly CSRedisClient RedisCache;
//         private readonly bool UseRedis = true;
//         private readonly int CacheTime = 1 * 60 * 60;
//         public GameController(IGameService gameService, IMemoryCache memory, CSRedisClient redisClient)
//         {
//             GameService = gameService;
//             this.MemoryCache = memory;
//             RedisCache = redisClient;
//         }
//         /// <summary>
//         /// 首发游戏
//         /// </summary>
//         /// <param name="type"></param>
//         /// <param name="platform"></param>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public MyResult<object> FristGame(int type, string platform)
//         {
//             string key = $"Game:FristGame_{type}_{platform}";
//             if (UseRedis)
//             {
//                 try
//                 {
//                     if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
//                     var cacheResult = GameService.FristGame(type, platform);
//                     var cacheString = cacheResult.ToJson(false, true, true);
//                     this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
//                     return cacheResult;
//                 }
//                 catch (Exception ex)
//                 {
//                     LogUtil<GameController>.Error(ex, "REDIS缓存错误");
//                     return GameService.FristGame(type, platform);
//                 }
//             }

//             if (this.MemoryCache.TryGetValue(key, out MyResult<object> result))
//             {
//                 return result;
//             }
//             result = GameService.FristGame(type, platform);
//             this.MemoryCache.Set(key, result, System.TimeSpan.FromSeconds(CacheTime));
//             return result;
//         }
//         /// <summary>
//         /// 游戏列表
//         /// </summary>
//         /// <param name="model"></param>
//         /// <returns></returns>
//         [HttpPost]
//         [AllowAnonymous]
//         public MyResult<object> GameList([FromBody]GameListDto model)
//         {
//             string key = $"Game:GameList_{model.Platform}_{model.Type}_{model.PageSize}_{model.PageIndex}";

//             if (UseRedis)
//             {
//                 try
//                 {
//                     if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
//                     var cacheResult = GameService.GameList(model);
//                     var cacheString = cacheResult.ToJson(false, true, true);
//                     this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
//                     return cacheResult;
//                 }
//                 catch (Exception ex)
//                 {
//                     LogUtil<GameController>.Error(ex, "REDIS缓存错误");
//                     return GameService.GameList(model);
//                 }
//             }

//             if (this.MemoryCache.TryGetValue(key, out MyResult<object> result))
//             {
//                 return result;
//             }
//             result = GameService.GameList(model);
//             this.MemoryCache.Set(key, result, System.TimeSpan.FromSeconds(CacheTime));
//             return result;
//         }
//         /// <summary>
//         /// 游戏详情
//         /// </summary>
//         /// <param name="gameId"></param>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public MyResult<object> GameDetail(string gameId)
//         {
//             if (string.IsNullOrWhiteSpace(gameId))
//             {
//                 return new MyResult();
//             }
//             string key = $"Game:GameDetail_{gameId}";

//             if (UseRedis)
//             {
//                 try
//                 {
//                     if (this.RedisCache.Exists(key)) { return this.RedisCache.Get<MyResult<object>>(key); }
//                     var cacheResult = GameService.GameDetail(gameId);
//                     var cacheString = cacheResult.ToJson(false, true, true);
//                     this.RedisCache.Set(key, cacheString, CacheTime, RedisExistence.Nx);
//                     return cacheResult;
//                 }
//                 catch (Exception ex)
//                 {
//                     LogUtil<GameController>.Error(ex, "REDIS缓存错误");
//                     return GameService.GameDetail(gameId);
//                 }
//             }

//             if (this.MemoryCache.TryGetValue(key, out MyResult<object> result))
//             {
//                 return result;
//             }
//             result = GameService.GameDetail(gameId);
//             if (result.Data != null)
//             {
//                 this.MemoryCache.Set(key, result, System.TimeSpan.FromSeconds(CacheTime));
//             }
//             return result;
//         }
//         /// <summary>
//         /// 闪电玩游戏授权
//         /// </summary>
//         /// <param name="sdwId"></param>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public MyResult<object> GenAuthSdwUrl(string sdwId)
//         {
//             return GameService.GenAuthSdwUrl(base.TokenModel.Id, sdwId);
//         }

//         /// <summary>
//         /// 游戏中心页面
//         /// </summary>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public IActionResult GenAuthSdwUrl2(int id)
//         {
//             var Result = GameService.GenAuthSdwUrl2(id);
//             if (Result.Data == null) { return Redirect("http://yoyoba.cn"); }
//             var RediectUri = Convert.ToString(Result.Data);
//             if (String.IsNullOrWhiteSpace(RediectUri)) { return Redirect("http://yoyoba.cn"); }
//             return Redirect(RediectUri);
//         }

//         /// <summary>
//         /// 游戏计时器
//         /// </summary>
//         /// <param name="sdwId"></param>
//         /// <returns></returns>
//         [HttpGet]
//         public MyResult<object> PlayTiming(string sdwId)
//         {
//             MyResult<object> result = new MyResult<object>();
//             try
//             {
//                 if (String.IsNullOrWhiteSpace(sdwId)) { return result; }
//                 if (this.TokenModel == null) { return result; }
//                 if (this.TokenModel.Id <= 0) { return result; }
//                 string CacheKey = $"GamePaly:{this.TokenModel.Id}";
//                 if (!RedisCache.Exists(CacheKey)) { return result; }

//                 #region 缓存内取出游戏时间
//                 string CacheString = RedisCache.Get(CacheKey);
//                 if (String.IsNullOrWhiteSpace(CacheString)) { return result; }
//                 var SplitArr = CacheString.Split("|");
//                 if (SplitArr.Length != 2) { return result; }
//                 if (!sdwId.Equals(SplitArr[0])) { return result; }
//                 if (!int.TryParse(SplitArr[1], out int timer)) { return result; }
//                 #endregion

//                 #region 推送游戏记录
//                 var SetTime = timer.UnixToDateTime();
//                 var PlayTime = (int)(DateTime.Now - SetTime).TotalSeconds;
//                 if (PlayTime >= 59)
//                 {
//                     var bstr = new { UserId = this.TokenModel.Id, CarryOut = 1, TaskType = 4, Devote = 0 }.GetJson();
//                     var b = RedisCache.Publish("YoYo_Member_DoSysTask", bstr);
//                 }
//                 RedisCache.Set(CacheKey, sdwId + "|" + DateTime.Now.ToUnixTime(), 150);
//                 #endregion

//                 return result;
//             }
//             catch { return result; }
//         }

//         [HttpPost]
//         public MyResult<object> WatchVedio([FromBody]VedioPost req)
//         {
//             MyResult<object> result = new MyResult<object>();
//             try
//             {
//                 if (String.IsNullOrWhiteSpace(req.postId) || String.IsNullOrWhiteSpace(req.imei)) { return result.SetStatus(ErrorCode.InvalidData, "无效数据"); }
//                 if (this.TokenModel == null) { return result; }
//                 if (this.TokenModel.Id <= 0) { return result; }

//                 string CacheKey = $"SystemVedio:{req.imei}";
//                 if (RedisCache.Exists(CacheKey)) { return result.SetStatus(ErrorCode.InvalidData, "请稍后观看"); ; }
//                 #region 推送视频记录
//                 var bstr = new { UserId = this.TokenModel.Id, CarryOut = 1, TaskType = 7, Devote = 0 }.GetJson();
//                 var b = RedisCache.Publish("YoYo_Member_DoSysTask", bstr);
//                 RedisCache.Set(CacheKey, req.postId + "_" + req.imei, 20);
//                 #endregion

//                 return result;
//             }
//             catch { return result.SetStatus(ErrorCode.InvalidData, "系统异常"); ; }
//         }

//         /// <summary>
//         /// 查询sdw支付订单
//         /// </summary>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public MyResult<object> QueryPayByChannel()
//         {
//             return GameService.QueryPayByChannel();//
//         }



//         public class VedioPost
//         {
//             public string postId { get; set; }

//             public string imei { get; set; }
//         }
//     }
// }