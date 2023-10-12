using domain.lfexentitys;
using domain.models;
using domain.models.equity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace domain.repository
{
    /// <summary>
    /// 股权
    /// </summary>
    public interface IEquityService
    {
        /// <summary>
        /// 获取股权信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<EquityInfo>> EquityPage(Int64 UserId);

        /// <summary>
        /// 获取股权信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<UserAccountEquity>> EquityInfo(long userId);

        /// <summary>
        /// 股权兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ExchangeEquity(EquityExchange exchange);

        /// <summary>
        /// 股权转让
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        Task<MyResult<Object>> TransferEquity(EquityTransfer transfer);

        /// <summary>
        /// 股权记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAccountEquityRecord>>> EquityRecords(QueryModel query);

        /// <summary>
        /// 股权冻结
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        Task<MyResult<object>> FrozenEquity(long userId, decimal Amount);
    }
}
