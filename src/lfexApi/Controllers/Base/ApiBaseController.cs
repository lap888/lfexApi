using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using domain.configs;
using domain.enums;
using domain.models;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace yoyoApi.Controllers.Base
{
    [Route("api/[controller]")]
    public class ApiBaseController : Controller
    {
        SortedDictionary<string, string> ReqParams = new SortedDictionary<string, string>();
        protected const string TOKEN_NAME = "token";
        protected const string Sign = "sign";
        protected const string TimeSpan = "timeSpan";
        protected SourceType SourceType { get; set; }
        protected TokenModel TokenModel { get; set; }

        public override Task OnActionExecutionAsync(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context, Microsoft.AspNetCore.Mvc.Filters.ActionExecutionDelegate next)
        {
            try
            {
                var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();
                //if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
                //{
                //    context.Result = new ObjectResult(new MyResult<object>().SetStatus(ErrorCode.Unauthorized, "你涉嫌非法入侵已经被我方风控系统抓捕稍后会有官方人员和你取得联系"));
                //    return base.OnActionExecutionAsync(context, next);
                //}
                if (userAgent.Contains("MicroMessenger"))
                {
                    SourceType = SourceType.WeChatApp;
                }
                else if (userAgent.Contains("iPhone") || userAgent.Contains("iPod") || userAgent.Contains("iPad"))
                {
                    SourceType = SourceType.IOS;
                }
                else if (userAgent.Contains("Android"))
                {
                    SourceType = SourceType.Android;
                }
                else
                {
                    SourceType = SourceType.Web;//SourceType.Unknown;
                }
                var _token = context.HttpContext.Request.Headers["token"].ToString();
                if (!string.IsNullOrEmpty(_token))
                {
                    ReqParams[TOKEN_NAME] = _token;
                }
                var _sign = context.HttpContext.Request.Headers["sign"].ToString();
                if (!string.IsNullOrEmpty(_sign))
                {
                    ReqParams[Sign] = _sign;
                }
                var _timeSpan = context.HttpContext.Request.Headers["timeSpan"].ToString();
                if (!string.IsNullOrEmpty(_timeSpan))
                {
                    ReqParams[TimeSpan] = _timeSpan;
                }
                foreach (var kv in context.HttpContext.Request.Query)
                {
                    ReqParams[kv.Key] = kv.Value.ToString();
                }
                if (context.HttpContext.Request.HasFormContentType)
                {
                    foreach (var kv in context.HttpContext.Request.Form)
                    {
                        ReqParams[kv.Key] = kv.Value.ToString();
                    }
                }
                if (context.HttpContext.Request.Method.Equals("Post") || context.HttpContext.Request.Method.Equals("POST") || context.HttpContext.Request.Method.Equals("post"))
                {
                    try
                    {
                        var values = context.HttpContext.GetContextDict();
                        foreach (var kv in values)
                        {
                            ReqParams[kv.Key] = kv.Value.ToString();
                        }
                    }
                    catch (System.Exception)
                    {
                        var log = ReqParams.GetJson();
                        var apiName = context.HttpContext.Request.Path.Value;
                        LogUtil<ApiBaseController>.Warn($"参数异常抓捕==={apiName}\r\n==={log}===\r\n{context.HttpContext.Request.Headers.GetJson()}");
                        context.Result = new ObjectResult(new MyResult<object>().SetStatus(ErrorCode.Unauthorized, "你涉嫌非法入侵已经被我方风控系统抓捕稍后会有官方人员和你取得联系"));
                    }
                }

                if (SourceType == SourceType.Unknown)
                {
                    context.Result = new ObjectResult(new MyResult<object>().SetStatus(ErrorCode.Unauthorized, "请设置User-Agent请求头: 如:iPhone 或者 Android 或则web"));
                }
                else
                {
                    var token = string.Empty;
                    var sign = string.Empty;
                    var timeSpan = string.Empty;
                    if (ReqParams.ContainsKey(TOKEN_NAME))
                    {
                        token = ReqParams[TOKEN_NAME];
                    }
                    if (ReqParams.ContainsKey(Sign))
                    {
                        sign = ReqParams[Sign];
                    }
                    if (ReqParams.ContainsKey(TimeSpan))
                    {
                        timeSpan = ReqParams[TimeSpan];
                    }
                    //can get token from server redis now only get form params
                    // ..
                    //
                    if (!context.ActionDescriptor.FilterDescriptors.Any(t => t.Filter is AllowAnonymousFilter))//need check token
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            context.Result = new ObjectResult(new MyResult<object>(ErrorCode.ReLogin, "token is empty you are error！"));
                        }
                        else if (string.IsNullOrEmpty(sign))
                        {
                            context.Result = new ObjectResult(new MyResult<object>(ErrorCode.ReLogin, "sign is empty you are error！"));
                        }
                        else
                        {
                            var SignUrl = context.HttpContext.Request.Path.HasValue ? context.HttpContext.Request.Path.Value.TrimStart('/') : "";
                            SignUrl += context.HttpContext.Request.QueryString.HasValue ? System.Web.HttpUtility.UrlDecode(context.HttpContext.Request.QueryString.Value) : "";
                            var model = CheckToken(token, SignUrl, sign, timeSpan);
                            if (model.Success)
                            {

                            }
                            if (!model.Success)
                            {
                                context.Result = new ObjectResult(model);
                            }

                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            TokenModel = new TokenModel();
                        }
                        else
                        {
                            try
                            {
                                if (!Yoyo.Core.AuthToken.CheckToken(token))
                                {
                                    context.Result = new ObjectResult(new MyResult<object>(ErrorCode.ReLogin, $"Token失效请重新登录"));
                                }
                                TokenModel = Yoyo.Core.AuthToken.GetToken<TokenModel>(token);
                                if (TokenModel == null)
                                {
                                    TokenModel = new TokenModel
                                    {
                                        Id = -1
                                    };
                                }
                                else
                                {
                                    if (TokenModel.Id == 0) { TokenModel.Id = -1; }
                                }
                            }
                            catch (System.Exception)
                            {
                                context.Result = new ObjectResult(new MyResult<object>(ErrorCode.ReLogin, $"Token失效请重新登录"));
                            }
                        }
                    }
                }

            }
            catch (System.Exception ex)
            {
                LogUtil<ApiBaseController>.Error(ex, ex.Message);
                context.Result = new ObjectResult(new MyResult<object>(ErrorCode.ReLogin, $"Token失效请重新登录"));
            }
            return base.OnActionExecutionAsync(context, next);
        }

        private MyResult<object> CheckToken(string token, string signUrl, string sign, string timeSpan)
        {
            MyResult<object> result = new MyResult<object>();
            try
            {
                //sign==
                if (string.IsNullOrWhiteSpace(timeSpan)) { return result.SetStatus(ErrorCode.ReLogin, "sign error 请联系管理员"); }
                if (!long.TryParse(timeSpan, out long miniTime))
                {
                    return result.SetStatus(ErrorCode.ReLogin, "sign error 请联系管理员L");
                }
                if ((UnixTime() - miniTime) > (60 * 1000 * 5) || (UnixTime() - miniTime) < -(60 * 1000 * 5))
                {
                    return result.SetStatus(ErrorCode.ReLogin, "sign error 请联系管理员T");
                }
                if (!SecurityUtil.ValidSign(sign, signUrl, token, Constants.YoyoKey, timeSpan))
                {
                    return result.SetStatus(ErrorCode.ReLogin, "sign error 请联系管理员V");
                }
                //token==

                if (!Yoyo.Core.AuthToken.CheckToken(token))
                {
                    return result.SetStatus(ErrorCode.ReLogin, "token error 请重新登录");
                }
                TokenModel = Yoyo.Core.AuthToken.GetToken<TokenModel>(token);
                if (TokenModel == null)
                {
                    return result.SetStatus(ErrorCode.ReLogin, "非法token");
                }
                if (TokenModel.Id < 1)
                {
                    return result.SetStatus(ErrorCode.ReLogin, "无效token");
                }

            }
            catch
            {
                return result.SetStatus(ErrorCode.ReLogin, $"Token失效请重新登录");
            }
            return result;
        }

        #region 时间转换微时间戳
        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <returns></returns>
        private static long UnixTime()
        {
            DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);
            long timeStamp = Convert.ToInt64((DateTime.Now - dateStart).TotalMilliseconds);
            return timeStamp;
        }
        #endregion

        public override void OnActionExecuted(Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                context.ExceptionHandled = true;
                if (context.HttpContext.IsAjaxRequest())
                {
                    LogUtil<ApiBaseController>.Error(context.Exception, context.Exception.Message);
#if DEBUG
                    context.Result = Json(new MyResult<object>(context.Exception, true));
#else
                    context.Result = Json(new MyResult<object>(context.Exception));
#endif
                }
                else
                {
#if DEBUG
                    context.Result = View("Error", new MyResult<object>(context.Exception, true));
#else
                    context.Result = View("Error", new MyResult<object>(context.Exception));
#endif
                }
            }
            base.OnActionExecuted(context);
        }
    }
}