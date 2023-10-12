using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using application;
using CSRedis;
using domain.models.yoyoDto;
using domain.repository;
using infrastructure.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yoyo.Core;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class UserAliController : ApiBaseController
    {
        public IAliPayAction AliPay { get; set; }
        public readonly CSRedisClient RedisCache;
        private readonly IQCloudPlugin QCloudSub;
        public UserAliController(IAliPayAction yoyoali, IQCloudPlugin qCloud, CSRedisClient redisClient)
        {
            AliPay = yoyoali;
            QCloudSub = qCloud;
            RedisCache = redisClient;
        }

        /// <summary>
        /// 进行二次实名认证
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Auth()
        {
            return await Task.Run(() =>
            {
                return new MyResult<object> { Code = -1, Message = "暂定二次认证" };
            });
            //String CacheKey = $"AliAction_Auth:{base.TokenModel.Id}";
            //if (RedisCache.Exists(CacheKey)) { return new MyResult<object> { Code = -1, Message = "请勿重复提交" }; }
            //var Result = await AliPay.CreatePayUrl(base.TokenModel.Id, 0.10M, ActionType.AUTH_ALIPAY);
            //RedisCache.Set(CacheKey, Result.Data == null ? "" : Result.Data, 30);
            //return Result;
        }

        /// <summary>
        /// 更换支付宝
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Change([FromBody] AliInfo req)
        {
            if (!DataValidUtil.IsEMail(req.Alipay) && !DataValidUtil.IsMobile(req.Alipay))
            {
                return new MyResult<object>(-1, "请求输入正常的支付宝号");
            }
            
            if (!string.IsNullOrEmpty(req.AlipayPic) && req.AlipayPic.Length > 1000)
            {
                try
                {
                    String BasePic = req.AlipayPic;
                    String FilePath = PathUtil.Combine("AlipayPic", SecurityUtil.MD5(base.TokenModel.Id.ToString()).ToLower() + ".png");
                    Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                    Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                    Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                    Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                    var newBase64 = reg1.Replace(BasePic, "+");
                    newBase64 = reg2.Replace(newBase64, "/");
                    newBase64 = reg3.Replace(newBase64, "=");
                    BasePic = reg4.Replace(newBase64, "");

                    byte[] bt = Convert.FromBase64String(BasePic);
                    await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                    req.AlipayPic = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    SystemLog.Debug(ex);
                    return new MyResult<object>() { Code = -1, Message = "头像上传失败" };
                }
            }
            return await AliPay.ModifyAlipay(base.TokenModel.Id, req.Alipay, req.PayPwd, req.AlipayPic);
        }


        public class AliInfo
        {
            public String Alipay { get; set; }

            public String PayPwd { get; set; }

            public string AlipayPic { get; set; }
        }
    }
}