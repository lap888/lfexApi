using application;
using application.Request;
using application.Response;
using CSRedis;
using domain.configs;
using domain.enums;
using domain.models;
using domain.models.yoyoDto;
using domain.repository;
using domain.lfexentitys;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    [Route("api/[action]")]
    [Produces("application/json")]
    public class UserController : ApiBaseController
    {
        private readonly IRealVerify RealVerify;
        private readonly IQCloudPlugin QCloudSub;
        private readonly CSRedisClient RedisCache;
        private readonly IUserWalletAccountService WalletAccount;
        public IYoyoUserSerivce YoyoUserSerivce { get; set; }
        public UserController(IYoyoUserSerivce yoyoUserSerivce, IRealVerify realVerify, IQCloudPlugin qCloud, CSRedisClient redisClient, IUserWalletAccountService userWallet)
        {
            YoyoUserSerivce = yoyoUserSerivce;
            RealVerify = realVerify;
            QCloudSub = qCloud;
            RedisCache = redisClient;
            WalletAccount = userWallet;
        }

        /// <summary>
        /// 生成支付签名
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> GenerateAppUrl()
        {
            return new MyResult<object> { Code = -1, Message = "系统维护中..." };
            // return await YoyoUserSerivce.GenerateAppUrl(base.TokenModel.Id);
        }

        /// <summary>
        /// 认证失败退款接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> PayRefund()
        {
            return await YoyoUserSerivce.PayRefund(base.TokenModel.Id);
        }

        /// <summary>
        /// 检查是否有订单
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> HavePayOrder()
        {
            return await YoyoUserSerivce.HavePayOrder(base.TokenModel.Id);
        }

        /// <summary>
        /// 支付通知
        /// </summary>
        /// <param name="outTradeNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> PayFlag(string outTradeNo)
        {
            return await YoyoUserSerivce.PayFlag(base.TokenModel.Id, outTradeNo);
        }

        /// <summary>
        /// 发送验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> SendVcode([FromBody] SendVcode model)
        {
            return await YoyoUserSerivce.SendVcode(model);
        }

        #region 校验验证码---已废弃
        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public MyResult<object> ConfirmVcode([FromBody] ConfirmVcode model)
        {
            return new MyResult<object>();
        }
        #endregion

        /// <summary>
        /// 确认邀请人
        /// </summary>
        /// <param name="mobile">邀请手机号</param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> GetNameByMobile(string mobile)
        {
            if (String.IsNullOrWhiteSpace(mobile))
            {
                return new MyResult().SetStatus(ErrorCode.NotFound, "邀请码有误 请联系推荐人");
            }
            var key = $"UserRcode:{mobile}";
            var result = RedisCache.Get<MyResult<object>>(key);
            if (null != result) { return result; }
            result = YoyoUserSerivce.GetNameByMobile(mobile);
            if (null != result.Data) { RedisCache.Set(key, result); }
            return result;

        }

        /// <summary>
        /// 确认 手机号 归属
        /// </summary>
        /// <param name="mobile">邀请手机号</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> GetUserByMobile(string mobile)
        {
            return await YoyoUserSerivce.GetUserByMobile(mobile);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> SignUp([FromBody] SignUpDto model)
        {
            return await YoyoUserSerivce.SignUp(model);
        }
        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> ForgetLoginPwd([FromBody] ConfirmVcode model)
        {
            return await YoyoUserSerivce.ForgetLoginPwd(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public MyResult<object> Login([FromBody] YoyoUserDto model)
        {
            return YoyoUserSerivce.Login(model);
        }
        /// <summary>
        /// 钱包金额
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> WalletAmount()
        {
            return YoyoUserSerivce.WalletAmount(base.TokenModel.Id);
        }
        /// <summary>
        /// 获取团队信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> TeamInfos([FromBody] TeamInfosReqDto model)
        {
            return await YoyoUserSerivce.TeamInfos(model, base.TokenModel.Id, base.TokenModel.Mobile);
        }

        /// <summary>
        /// 更新团队人数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> UpdateTeamCount()
        {
            return YoyoUserSerivce.UpdateTeamCount();
        }
        /// <summary>
        /// 通过实名更新团队活果核
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> UpdateTeamCandyHByAuth()
        {
            return YoyoUserSerivce.UpdateTeamCandyHByAuth();
        }
        /// <summary>
        /// 根据新增任务添加团队果核
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> AddTeamCandyHByTask()
        {
            return YoyoUserSerivce.AddTeamCandyHByTask();
        }
        /// <summary>
        /// 根据过期任务减活跃度
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public MyResult<object> SubTeamCandyHByTask()
        {
            return YoyoUserSerivce.SubTeamCandyHByTask();
        }
        /// <summary>
        /// 认证信息记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Authentication([FromBody] AuthenticationDto model)
        {
            MyResult result = new MyResult();
            if (String.IsNullOrWhiteSpace(model.CertifyId))
            {
                return new MyResult<object>(-1, "请下载最新版APP进行实名认证！");
            }
            FaceInitRecord InitRecord = await YoyoUserSerivce.VerfyFaceInit(model);
            if (InitRecord == null)
            {
                LogUtil<UserController>.Error("实名认证信息过期日志记录:\r\n" + JsonConvert.SerializeObject(model));
                return new MyResult<object>(-1, "认证已过期");
            }
            if (!InitRecord.IdcardNum.Equals(model.IdNum, StringComparison.OrdinalIgnoreCase))
            {
                return new MyResult<object>(-1, "您提交的认证信息不匹配");
            }
            if (!InitRecord.Alipay.Equals(model.Alipay))
            {
                return new MyResult<object>(-1, "您提交的支付宝信息不匹配");
            }
            if (InitRecord.IsUsed != 0)
            {
                return new MyResult<object>(-1, "您已完成实名认证");
            }
            RspRealVerifyQuery rult = await RealVerify.Execute(new ReqRealVerifyQuery()
            {
                SceneId = "1000000095",
                CertifyId = model.CertifyId
            });

            if (rult.IsError)
            {
                result.Code = -1;
                result.Message = rult.ErrMsg;
                return result;
            }
            if (rult.Data.IsPass)
            {
                return YoyoUserSerivce.Authentication(model, base.TokenModel.Id);
            }
            return new MyResult<object>(-1, "实名认证失败");
        }
        /// <summary>
        /// 修改用户头像
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> ModifyUserPic([FromBody] YoyoUserDto model)
        {
            if (!string.IsNullOrEmpty(model.UserPic) && model.UserPic.Length > 1000)
            {
                try
                {
                    String BasePic = model.UserPic;
                    String FilePath = PathUtil.Combine("HeadImg", SecurityUtil.MD5(base.TokenModel.Id.ToString()).ToLower() + ".png");
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
                    model.UserPic = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    LogUtil<UserController>.Debug(ex, ex.Message);
                    return new MyResult<object>() { Code = -1, Message = "头像上传失败" };
                }
                //Int32 fileName = base.TokenModel.Id;
                //model.UserPic = ImageHandlerUtil.SaveBase64Image(model.UserPic, $"{fileName}.png", Constants.USER_PIC);
            }
            return YoyoUserSerivce.ModifyUserPic(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 修改用户昵称
        /// </summary>
        /// <param name="name">昵称</param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ModifyUserName(string name)
        {
            return YoyoUserSerivce.ModifyUserName(name, base.TokenModel.Id);
        }

        /// <summary>
        /// 修改邀请码
        /// </summary>
        /// <param name="name">code</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> ModifyUserInviterCode(string name)
        {
            return await YoyoUserSerivce.ModifyUserInviterCode(name, base.TokenModel.Id);
        }
        /// <summary>
        /// 修改登陆密码
        /// </summary>
        /// <param name="oldPwd"></param>
        /// <param name="newPwd"></param>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd)
        {
            return YoyoUserSerivce.ModifyLoginPwd(oldPwd, newPwd, base.TokenModel.Id);
        }
        /// <summary>
        /// 修改交易密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> ModifyOtcPwd([FromBody] ModifyOtcPwdDto model)
        {
            return await YoyoUserSerivce.ModifyOtcPwd(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 提交人工审核
        /// /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> AdminAuth([FromBody] AuthenticationDto model)
        {
            MyResult result = new MyResult();
            // return result.SetStatus(ErrorCode.InvalidData, "人工审核暂停开放");
            if (this.TokenModel.Id < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.NegativeUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证反面不能为空");
            }
            if (string.IsNullOrEmpty(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝号不能为空");
            }
            if (string.IsNullOrEmpty(model.CharacterUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "手持身份证不能为空");
            }
            if (string.IsNullOrEmpty(model.PositiveUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证正面不能为空");
            }
            if (string.IsNullOrEmpty(model.IdNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证号不能为空");
            }
            if (string.IsNullOrEmpty(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝不能为空");
            }
            if (string.IsNullOrEmpty(model.TrueName))
            {
                return result.SetStatus(ErrorCode.InvalidData, "姓名不能");
            }
            return await YoyoUserSerivce.AdminAuth(model, base.TokenModel.Id);
        }
        /// <summary>
        /// 浏览广告获取果皮
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public MyResult<object> LookAdGetCandyP(int id)
        {
            return YoyoUserSerivce.LookAdGetCandyP(id, base.TokenModel.Id);

        }

        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        public async Task<MyResult<object>> UnbindDevice([FromBody] UnbindDto device)
        {
            return await YoyoUserSerivce.UnbindDevice(device);
        }

        /// <summary>
        /// 设置联系方式
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> SetContact([FromBody] ContactModel contact)
        {
            contact.UserId = base.TokenModel.Id;
            return await YoyoUserSerivce.SetContact(contact);
        }

        /// <summary>
        /// 实名认证看广告
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<Object>> RealNameAd()
        {
            return await YoyoUserSerivce.RealNameAd(base.TokenModel.Id);
        }

        /// <summary>
        /// 解除交易封禁
        /// </summary>
        /// <param name="unblock"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<Object>> UnblockTrade([FromBody] UnblockTrade unblock)
        {
            return await YoyoUserSerivce.UnblockTrade(base.TokenModel.Id, unblock.PayPwd);
        }

        /// <summary>
        /// 我的活动券 (未实现)
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<List<ActivityCouponModel>>> ActivityCoupon([FromBody] QueryModel query)
        {
            return await Task.Run(() =>
            {
                return new MyResult<List<ActivityCouponModel>>();
            });
        }



    }
    /// <summary>
    /// 支付模型
    /// </summary>
    public class UnblockTrade
    {
        /// <summary>
        /// 支付密码
        /// </summary>
        public String PayPwd { get; set; }
    }
}