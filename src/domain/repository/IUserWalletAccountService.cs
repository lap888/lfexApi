using domain.enums;
using domain.lfexentitys;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace domain.repository
{
    public interface IUserWalletAccountService
    {
        /// <summary>
        /// 初始化钱包
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> InitWalletAccount(long userId);
        /// <summary>
        /// 获取钱包信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<UserAccountWallet>> WalletAccountInfo(long userId);
        /// <summary>
        /// 获取钱包流水
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAccountWalletRecord>>> WalletAccountRecord(long userId, int PageIndex = 1, int PageSize = 20, AccountModifyType ModifyType = AccountModifyType.ALL);
        /// <summary>
        /// 钱包账户余额冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        Task<MyResult<object>> FrozenWalletAmount(long userId, decimal Amount);
        /// <summary>
        /// 钱包账户余额变动
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="modifyType"></param>
        /// <param name="useFrozen">使用冻结金额</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        Task<MyResult<object>> ChangeWalletAmount(long userId, decimal Amount, AccountModifyType modifyType, bool useFrozen, params string[] Desc);
        
        /// <summary>
        /// 现金钱包取现
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="TradePwd"></param>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        Task<MyResult<object>> Withdraw(int userId, decimal Amount, string TradePwd, string TradeNo);
    }
}
