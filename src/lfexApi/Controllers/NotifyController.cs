using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using application;
using application.Request;
using application.Response;
using application.Utils;
using domain.models;
using domain.repository;
using infrastructure.utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Yoyo.Core;

namespace yoyoApi.Controllers
{
    /// <summary>
    /// 异步通知
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class NotifyController : ControllerBase
    {
        private readonly IYoyoUserSerivce YoyoUserSerivce;
        private readonly IRechargeService RechargeSub;
        private readonly IAliPayAction AliAction;
        public NotifyController(IYoyoUserSerivce yoyoUserSerivce, IRechargeService recharge, IAliPayAction alipay)
        {
            YoyoUserSerivce = yoyoUserSerivce;
            RechargeSub = recharge;
            AliAction = alipay;
        }

        [HttpPost]
        public async Task<IActionResult> Ali()
        {
            Dictionary<string, string> keys = new Dictionary<string, string>();
            try
            {
                ICollection<string> requestItem = Request.Form.Keys;
                foreach (var item in requestItem) { keys.Add(item, Request.Form[item]); }
                if (!keys.TryGetValue("out_trade_no", out string out_trade_no)) { return Content("fail"); }
                if (String.IsNullOrWhiteSpace(out_trade_no)) { return Content("fail"); }
                if (!keys.TryGetValue("trade_status", out string trade_status)) { return Content("fail"); }
                if (String.IsNullOrWhiteSpace(trade_status)) { return Content("fail"); }
                if (!trade_status.Equals("TRADE_SUCCESS")) { return Content("fail"); }
                keys.TryGetValue("passback_params", out string passback_params);
                if (String.IsNullOrWhiteSpace(passback_params)) { passback_params = String.Empty; }
                if (passback_params.Equals(domain.models.yoyoDto.ActionType.AUTH_ALIPAY.ToString()))
                {
                    return Content(await AliAction.AuthAliPay(out_trade_no));
                }
                if (passback_params.Equals(domain.models.yoyoDto.ActionType.CHANGE_ALIPAY.ToString()))
                {
                    return Content(await AliAction.ChangeAliPay(out_trade_no));
                }
                if (passback_params.Equals(domain.models.yoyoDto.ActionType.CASH_RECHARGE.ToString()))
                {
                    return Content(await AliAction.CashRecharge(out_trade_no));
                }
                return Content(await YoyoUserSerivce.AliNotify(out_trade_no));
            }
            catch (Exception ex)
            {
                LogUtil<NotifyController>.Error(ex, "异步通知异常:\r\n" + keys.ToJson());
                return Content("fail");
            }

        }

        /// <summary>
        /// 充值结果通知
        /// </summary>
        /// <param name="notify"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> FuluRecharge([FromBody] RechargeNotify notify)
        {
            var rult = await RechargeSub.QueryOrder(notify.CustomerOrderNo);
            if (rult.Success) { return Content("success"); }
            return Content("failed");
        }
    }
}