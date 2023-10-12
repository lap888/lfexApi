using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using domain.models;
using domain.models.yoyoDto;
using domain.lfexentitys;
using domain.models.lfexDto;
using System.Data;
using domain.enums;

namespace domain.repository
{
    public interface ITradeService
    {
        //交易面板统计
        MyResult<models.yoyoDto.CoinTradeExt> GetTradeTotal(string coinType);

        /// <summary>
        /// 交易面板最新统计
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<models.yoyoDto.CoinTradeModelDto> CoinTradeTotal(string coinType, int userId);

        Task<MyResult<object>> FrozenWalletAmount(IDbTransaction OutTran, bool isUserOutTransaction, long userId, decimal Amount, string coinType);
        Task<MyResult<object>> ChangeWalletAmount(IDbTransaction OutTran, bool isUserOutTransaction, long userId, string coinType, decimal Amount, LfexCoinnModifyType modifyType, bool useFrozen, params string[] Desc);
        /// <summary>
        /// 买单单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<List<CoinTradeDto>>> TradeList(TradeReqDto model);
        /// <summary>
        /// 发布买单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> StartBuy(TradeDto model, int userId);
        /// <summary>
        /// 发布卖单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> StartSell(TradeDto model, int userId);
        //确认出售
        // MyResult<object> DealBuy(TradeDto model, int userId);
        /// <summary>
        /// 确认出售
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> DealBuy(TradeDto model, int userId);

        /// <summary>
        /// 确认购买
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> ConfirmBuy(TradeDto model, int userId);

        /// <summary>
        /// 取消买单
        /// </summary>
        /// <param name="title"></param>
        /// <param name="orderNum"></param>
        /// <param name="tradePwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> CancleTrade(string title, string orderNum, string tradePwd, int userId);

        /// <summary>
        /// 我的交易订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<List<TradeListReturnDto>>> MyTradeList(MyTradeListDto model, int userId);

        Task<MyResult<object>> MyTradeListWt(string coinType, int userId);

        //确认支付
        // MyResult<object> Paid(PaidDto model, int userId);
        Task<MyResult<object>> Paid(PaidDto model, int userId);
        //确认收款 发送糖果
        Task<MyResult<object>> PaidCoin(PaidDto model, int userId);
        //强制发送糖果
        MyResult<object> ForcePaidCoin();
        //申诉
        MyResult<object> CreateAppeal(CreateAppealDto model, int userId);

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<ListModel<TradeOrder>>> QueryTradeOrder(QueryTradeOrder query);

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> CloseTradeOrder(TradeOrder order);

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> ResumeTradeOrder(TradeOrder order);

        /// <summary>
        /// 恢复订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> BanBuyer(TradeOrder order);

        /// <summary>
        /// 解除超时封禁
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<object>> Unblock(TradeOrder order);

        /// <summary>
        /// 查看申诉
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        Task<MyResult<List<TradeAppeals>>> ViewAppeal(TradeOrder order);
    }
}