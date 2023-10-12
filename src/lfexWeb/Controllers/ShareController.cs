using System;
using System.Text.RegularExpressions;
using System.Web;
using application;
using CSRedis;
using domain.repository;
using domain.lfexentitys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace webAdmin.Controllers
{
    public class ShareController : Controller
    {
        private readonly ISystemService SystemService;
        private readonly IYoyoUserSerivce UserService;
        private readonly IWechatPlugin WechatPlugin;
        private readonly IMemoryCache MemoryCache;
        private readonly CSRedisClient RedisCache;
        public ShareController(ISystemService system, IYoyoUserSerivce userSerivce, IWechatPlugin wechatPlugin, IMemoryCache memory, CSRedisClient redis)
        {
            SystemService = system;
            UserService = userSerivce;
            WechatPlugin = wechatPlugin;
            RedisCache = redis;
            MemoryCache = memory;
        }
        [Route("[controller]/{id}.html")]
        [Route("[controller]/{id}/{rcode}.html")]
        public IActionResult Index(int id, string rcode = "", string code = "")
        {
            bool IsWeChat = HttpContext.Request.Headers["User-Agent"].ToString()
                .Contains("micromessenger", StringComparison.OrdinalIgnoreCase);
            //======================判断你是否在微信浏览器内======================//
            if (IsWeChat && String.IsNullOrWhiteSpace(code))
            {
                string Url = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host + HttpContext.Request.Path;
                return Redirect(WechatPlugin.GetCodeUrl(HttpUtility.UrlEncode(Url), Guid.NewGuid().ToString("N")));
            }
            //======================预定义前端使用信息======================//
            ViewData["userName"] = "哟哟吧";
            ViewData["headImage"] = "https://file.yoyoba.cn/images/avatar/default/1.png";
            ViewData["code"] = "";
            ViewData["count"] = 0;
            ViewData["title"] = "";
            ViewData["data"] = "";
            ViewData["adImage"] = "";
            ViewData["goUrl"] = "";
            //======================获取推荐用户信息======================//
            String UserCode = rcode;
            if (!String.IsNullOrWhiteSpace(rcode))
            {
                User UserInfo;
                String CodeCacheKey = $"ClickUserInfo:{rcode}";
                if (RedisCache.Exists(CodeCacheKey)) { UserInfo = RedisCache.Get<User>(CodeCacheKey); } else { UserInfo = UserService.GetNameByRcode(rcode)?.Data; }
                if (null != UserInfo)
                {
                    RedisCache.Set(CodeCacheKey, UserInfo, 1200);
                    ViewData["userName"] = UserInfo.Name;
                    ViewData["headImage"] = "https://file.yoyoba.cn/" + UserInfo.AvatarUrl.TrimStart('/');
                    UserCode = String.IsNullOrWhiteSpace(UserInfo.Rcode) ? rcode : UserInfo.Rcode;
                }
            }
            ViewData["code"] = UserCode;
            String ErrorUrl = $"http://yoyoba.cn/down?code={UserCode}";

            //======================获取广告基本信息======================//
            SysBanner BannerResult = this.SystemService.Banner(id);
            if (null == BannerResult) { return Redirect(ErrorUrl); }
            ViewData["count"] = RedisCache.IncrBy($"ClickAdCount:{BannerResult.Id}");
            ViewData["title"] = BannerResult.Title;
            ViewData["data"] = BannerResult.Params;
            ViewData["adImage"] = "https://file.yoyoba.cn/" + BannerResult.ImageUrl.TrimStart('/');

            //======================解析广告内参数内容======================//
            try
            {
                dynamic dy = JsonConvert.DeserializeObject<dynamic>(BannerResult.Params);
                ViewData["goUrl"] = Convert.ToString(dy.url);
            }
            catch (Exception)
            {
                try
                {
                    String IframeUrl = String.Empty;
                    if (BannerResult.Params.Contains("<iframe", StringComparison.OrdinalIgnoreCase)) { IframeUrl = new Regex("src=\"(.*)\"").Match(BannerResult.Params).Groups["1"].Value; }
                    if (!String.IsNullOrWhiteSpace(IframeUrl)) { ViewData["goUrl"] = IframeUrl;  }
                }
                catch { }
            }

            //======================微信内发送赠送果皮消息到处理服务器======================//
            if (IsWeChat && !String.IsNullOrWhiteSpace(code) && !String.IsNullOrWhiteSpace(UserCode))
            {
                try
                {
                    String SendMsg = JsonConvert.SerializeObject(new { AdId = id, Code = code, UserCode = rcode });
                    RedisCache.Publish("YoYo_Member_AD_Share", SendMsg);
                }
                catch { }
            }
            return View();            
        }
    }
}