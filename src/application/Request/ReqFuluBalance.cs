using application.Response;
using application.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace application.Request
{
    /// <summary>
    /// 获取账户余额
    /// </summary>
    public class ReqFuluBalance : IFuluRequest<FuluResponse<RspFuluBalance>>
    {
        /// <summary>
        /// 获取接口方法
        /// </summary>
        /// <returns></returns>
        public string GetApiName()
        {
            return "fulu.user.info.get";
        }

        /// <summary>
        /// 请求内容
        /// </summary>
        /// <returns></returns>
        public string GetBizContent()
        {
            return "{}";
        }
    }
}
