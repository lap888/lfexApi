using domain.models.yoyoDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace domain.repository
{
    public interface IAliPayAction
    {
        Task<MyResult<object>> CreatePayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "");
        Task<String> ChangeAliPay(String TradeNo);
        Task<String> AuthAliPay(String TradeNo);

        Task<String> CashRecharge(String TradeNo);
        /// <summary>
        /// 修改支付宝账号
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Alipay"></param>
        /// <param name="PayPwd"></param>
        /// <param name="AlipayPic"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ModifyAlipay(Int64 UserId, String Alipay, String PayPwd,String AlipayPic);
    }
}
