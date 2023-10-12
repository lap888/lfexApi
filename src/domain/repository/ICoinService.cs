using System.Collections.Generic;
using System.Threading.Tasks;
using domain.enums;
using domain.lfexentitys;
using domain.models;
using domain.models.lfexDto;

namespace domain.repository
{
    /// <summary>
    /// 小鱼交易所服务
    /// </summary>
    public interface ICoinService
    {
        /// <summary>
        /// K 线
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> KLine(QueryKLine model);

        /// <summary>
        /// K 线数据面板
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        Task<MyResult<object>> KLinePanel(string coinType);

        /// <summary>
        /// type =0 委托订单 type =1 最新成交  type=2 币种简介
        /// </summary>
        /// <param name="type"></param>
        /// <param name="coinType"></param>
        /// <returns></returns>
        Task<MyResult<object>> CoinData(int type, string coinType);

        Task<MyResult<object>> NewCoinData(string coinType);




        /// <summary>
        /// 获取币排行榜FindCoinRank
        /// 1涨幅榜-2跌幅榜-3成交榜
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<List<CoinTypeModel>>> FindCoinRank(QueryCoinRank model);
        /// <summary>
        /// 获取币种
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<List<CoinTypeModel>>> FindCoinType(int userId);

        /// <summary>
        /// 获取币资产
        /// </summary>
        /// <param name="type">0币币 1发币</param>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<MyResult<CoinUserAccountWallet>> FindCoinAmount(int type, long userId);

        /// <summary>
        /// 获取数字资产明细
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAccountWalletRecord>>> CoinAccountRecord(long accountId, long userId, int PageIndex = 1, int PageSize = 20, LfexCoinnModifyType ModifyType = LfexCoinnModifyType.ALL);

        /// <summary>
        /// 查单接口
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<CoinMoveRecord>> FindOrder(FindOrderModel model);

        /// <summary>
        /// 小鱼交易所提币到其他平台
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> MoveCoinToSomeone(MoveCoinToSomeoneModel model, int userId);

        //
        /// <summary>
        /// 第三方转平台币入交易所接口
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> SomeoneMoveCoinTome(SomeMoveCoinToMeModel model);

        /// <summary>
        /// 校验adress
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<object>> CheckAdress(CheckAdressModel model);

        /// <summary>
        /// 提币发送验证码
        /// </summary>
        /// <param name="mobile"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> MoveCoinSendCode(string mobile, int userId);
        /// <summary>
        /// 矿机列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<List<MinningDto>>> MinningList(int userId);
        /// <summary>
        /// 挖矿
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mId"></param>
        /// <returns></returns>
        Task<MyResult<object>> DoTask(int userId, int mId);
        /// <summary>
        /// 维修矿机
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mId"></param>
        /// <returns></returns>
        Task<MyResult<object>> RepairMinning(int userId, int mId);
        /// <summary>
        /// 贡献值流水
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> GlodsRecord(BaseModel model, int userId);

        // 锁仓配置
        Task<MyResult<object>> LookUpIncomeSetting();

        //确认锁仓
        Task<MyResult<object>> ConfirmLookUp(int userId, int type, decimal amount);

        //矿池订单
        Task<MyResult<object>> MinnersOrder(LooKUpMinnerModel model, int userId);
        //赎回
        Task<MyResult<object>> SopOrder(int userId, string orderNum);




    }
}