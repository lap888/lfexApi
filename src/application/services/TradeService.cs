using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.models;
using domain.models.yoyoDto;
using domain.repository;
using domain.lfexentitys;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using Yoyo.Core;
using domain.models.lfexDto;
using Newtonsoft.Json;

namespace application.services
{
    public class TradeService : bases.BaseServiceLfex, ITradeService
    {
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";

        private readonly CSRedisClient RedisCache;
        private readonly HttpClient Client;
        private readonly IUserWalletAccountService WalletAccount;
        private readonly Models.AppSetting AppSetting;
        public TradeService(IHttpClientFactory factory, IUserWalletAccountService userWallet, IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient, IOptionsMonitor<Models.AppSetting> monitor) : base(connectionStringList)
        {
            RedisCache = redisClient;
            WalletAccount = userWallet;
            Client = factory.CreateClient("JPushSMS");
            AppSetting = monitor.CurrentValue;
        }

        /// <summary>
        /// 取消交易订单
        /// </summary>
        /// <param name="title"></param>
        /// <param name="orderNum"></param>
        /// <param name="tradePwd"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CancleTrade(string title, string orderNum, string tradePwd, int userId)
        {
            MyResult<object> result = new MyResult<object>();
            if (userId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            if (string.IsNullOrEmpty(tradePwd)) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空"); }
            if (string.IsNullOrEmpty(orderNum)) { return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空"); }
            if (!ProcessSqlStr(orderNum)) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }

            //用户信息
            var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
            if (SecurityUtil.MD5(tradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }

            CoinTrade TradeInfo = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select * from coin_trade where id ={orderNum};");
            if (TradeInfo.Status != 1) { return result.SetStatus(ErrorCode.SystemError, "订单状态异常"); }
            if (TradeInfo.BuyerUid == userInfo.Id)
            {
                if (title == "fabi")
                {
                    base.dbConnection.Execute($"update coin_trade set status=0 where id={orderNum};");//取消订单
                    result.Data = true;
                }
                else
                {
                    //bibi -解冻USDT
                    var res = await FrozenWalletAmount(null, false, userId, -(decimal)TradeInfo.TotalPrice, "USDT(ERC20)");
                    if (res.Code != 200)
                    {
                        return result.SetStatus(ErrorCode.SystemError, res.Message);
                    }
                    base.dbConnection.Execute($"update coin_trade set status=0 where id={orderNum};");//取消订单

                }
            }
            if (TradeInfo.SellerUid == userInfo.Id)
            {
                var res = await FrozenWalletAmount(null, false, userId, -(decimal)(TradeInfo.Amount + TradeInfo.Fee), TradeInfo.CoinType);
                if (res.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, res.Message);
                }
                base.dbConnection.Execute($"update coin_trade set status=0 where id={orderNum};");//取消订单
            }
            return result;
        }

        /// <summary>
        /// 订单申述
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> CreateAppeal(CreateAppealDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
            }
            if (string.IsNullOrEmpty(model.Description))
            {
                return result.SetStatus(ErrorCode.InvalidData, "申诉内容不能为空");
            }
            if (string.IsNullOrEmpty(model.PicUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "申诉凭据不能为空");
            }
            if (string.IsNullOrEmpty(model.OrderId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            var orderInfo = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select status from coin_trade where id ={model.OrderId}");
            if (orderInfo == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
            }
            //创建申诉记录 更改订单状态 写入消息通知
            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    //记录
                    base.dbConnection.Execute($"insert into appeals (orderId, description, picUrl) values ('{model.OrderId}', '{model.Description}', '{model.PicUrl}')", null, transaction);
                    //update
                    base.dbConnection.Execute($"update coin_trade set status=5, appealTime = now() where id='{model.OrderId}'", null, transaction);
                    //我的信息记录
                    var msg = $"您发起的{orderInfo.Amount}买单,卖家发起申诉";
                    base.dbConnection.Execute($"insert into notice_infos (userId, content, refId, type,title) values ({userId}, '{msg}', '{model.OrderId}', '1','申诉通知')", null, transaction);
                    result.Data = new { status = 5 };
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    LogUtil<TradeService>.Error(ex.Message);
                    transaction.Rollback();
                    return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                }
            }
            base.dbConnection.Close();
            return result;
        }

        /// <summary>
        /// 确认出售
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> DealBuy(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            if (!String.IsNullOrWhiteSpace(AppSetting.TradeTips))
            {
                return result.SetStatus(ErrorCode.InvalidData, AppSetting.TradeTips);
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                if (model == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "入参非法");
                }
                if (userId < 0)
                {
                    return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
                }

                if (string.IsNullOrEmpty(model.TradePwd))
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
                }
                if (string.IsNullOrEmpty(model.OrderNum))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
                }
                if (!ProcessSqlStr(model.OrderNum))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "非法操作");
                }
                if (model.CoinType == "USDT(ERC20)")
                {
                    if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 9)
                    {
                        return result.SetStatus(ErrorCode.TimeNoOpen, $"法币交易时间为每天9:00-22:00...");
                    }
                    // return result.SetStatus(ErrorCode.TimeNoOpen, $"法币交易系统维护中...");
                }
                if (DateTime.Now.TimeOfDay > AppSetting.TradeEndTime.TimeOfDay || DateTime.Now.TimeOfDay < AppSetting.TradeStartTime.TimeOfDay)
                {
                    return result.SetStatus(ErrorCode.TimeNoOpen, $"交易时间为每天{AppSetting.TradeStartTime.ToString("HH:mm")}-{AppSetting.TradeEndTime.ToString("HH:mm")}");
                }

                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"DealBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");

                #region  查询是否被封禁 (买方)
                //查询是否被封禁
                //用户信息
                var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
                if (userInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空");
                }
                //===糖果转入后24小时内禁止交易===//
                if (model.CoinType == "糖果" || model.CoinType == "YB")
                {
                    var inTime = base.dbConnection.QueryFirstOrDefault<DateTime>($"select uawr.`ModifyTime` from (select AccountId from `user_account_wallet` where `coinType`='{model.CoinType}' and userId={userId}) a left join `user_account_wallet_record` uawr on a.`AccountId`=uawr.`AccountId` where (`ModifyType`=24 or `ModifyType`=37) order by uawr.`RecordId` desc limit 1");
                    if (inTime.ToString("yyyy-MM-dd") != "0001-01-01" && userInfo.Type == 0)
                    {
                        if (DateTime.Now < inTime.AddHours(24))
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"转入{model.CoinType}24小时内，禁止交易，交易解禁时间为:{inTime.AddHours(24).ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }

                }

                var coinBalance = base.dbConnection.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_wallet` where `CoinType`='{model.CoinType}' and userId={userId}");
                var banOrderCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(*) as count from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}'");
                if (banOrderCount != 0 && userInfo.Type == 0)
                {
                    //最近一次封禁时间
                    var lastBanTime = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(1);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }

                #endregion
                //支付宝账号信息判断
                if (string.IsNullOrWhiteSpace(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您没有设置支付宝，买家无法打款给您。请完善信息后进行交易。");
                }
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (reg.IsMatch(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您的支付宝账号异常，请完善后进行交易。");
                }
                //查订单是否存在
                var order = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"SELECT `status`,coinType, buyerUid, amount, totalPrice, trendSide FROM coin_trade WHERE id = {model.OrderNum};");
                if (order == null) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单..."); }
                if (order.Status != 1) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单..."); }
                if (!order.TrendSide.Equals("BUY", StringComparison.OrdinalIgnoreCase)) { return result.SetStatus(ErrorCode.SystemError, "订单类型错误"); }
                if (userInfo.Id == 5124 && order.Amount > 500)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "卖单数量不能大于500...");
                }
                if (userInfo.Type == 1 && userInfo.Id != 2 && model.CoinType == "糖果")
                {
                    //查买方是不是商家
                    var userInfoBuy = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={order.BuyerUid}");
                    if (userInfoBuy != null && userInfoBuy.Type == 1)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "商家之间禁止交易糖果...");
                    }
                }
                //计算手续费
                decimal fee = 0;

                #region 计算新的等级
                string UserLevel = userInfo.Level.ToLower();

                #endregion

                #region 新的计算手续费
                if (userInfo.Level.ToLower().Equals("lv0")) { return result.SetStatus(ErrorCode.InvalidData, "LV0禁止交易"); }

                decimal SellRate = 0.01M;
                if (model.CoinType == "LF")
                {

                    if (userInfo.Level.ToLower().Equals("lv1"))
                    {
                        SellRate = 0.5M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv2"))
                    {
                        SellRate = 0.35M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv3"))
                    {
                        SellRate = 0.3M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv4"))
                    {
                        SellRate = 0.28M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv5"))
                    {
                        SellRate = 0.25M;
                    }
                    else
                    {
                        SellRate = 0.25M;
                    }
                }
                fee = order.Amount.Value * SellRate;
                #endregion

                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd)
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误");
                }
                //
                Decimal TotalCandy = fee + order.Amount.Value;

                if (coinBalance < TotalCandy)
                {
                    return result.SetStatus(ErrorCode.InvalidData, $"您当前{model.CoinType}为{coinBalance},不足以出售{model.Amount}个{model.CoinType}");
                }
                // if (userInfo.AuditState != 2)
                // {
                //     return result.SetStatus(ErrorCode.NoAuth, "没有实名认证");
                // }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 4 || userInfo.Status == 5)
                {
                    return result.SetStatus(ErrorCode.AccountDisabled, "账号已被封禁 请联系管理员");
                }

                #region 今日 是否存在未完成的订单
                StringBuilder QueryProgressOrderSql = new StringBuilder();
                QueryProgressOrderSql.Append($"SELECT count(id) AS count FROM coin_trade WHERE ( `sellerUid` = @UserId OR `buyerUid` = @UserId) and `CoinType`='{model.CoinType}' AND `status` IN ( 1, 2, 3, 5 );");
                var isHasOrder = base.dbConnection.QueryFirstOrDefault<int>(QueryProgressOrderSql.ToString(), new { UserId = userId });
                if (isHasOrder > 0 && userInfo.Type == 0)
                {
                    return result.SetStatus(ErrorCode.HasValued, "您今天还有订单未完成，请先完成当前订单再交易");
                }
                #endregion

                var orderCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(*) as count from coin_trade where sellerUid={userId} and `CoinType`='{model.CoinType}' and status=4 and to_days(dealTime)=to_days(now())");

                if (orderCount >= 3 && userInfo.Type == 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "出售次数受限，每日只可交易三次");
                }
                var buyerMobile = base.dbConnection.QueryFirstOrDefault<string>($"select mobile from user where id={order.BuyerUid}");

                //买家和卖家不能相同
                if (order.BuyerUid == userId)
                {
                    return result.SetStatus(ErrorCode.SystemError, "自己无法卖给自己!");
                }

                //冻结 更新用户余额 发记录 短信
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        #region bibi
                        if (model.Title == "bibi")
                        {
                            try
                            {
                                #region 划转
                                base.dbConnection.Execute($"update coin_trade set sellerUid = {userId},sellerAlipay='{userInfo.Alipay}',fee = {fee},dealTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',dealEndTime='{DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss")}', status = 4 where id = {model.OrderNum}", null, transaction);

                                //卖方扣币
                                var subSellerCoin = await ChangeWalletAmount(transaction, true, userId, model.CoinType, -Math.Round(TotalCandy, 4), LfexCoinnModifyType.Lf_Sell_Coin, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round(fee, 4).ToString());
                                if (subSellerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subSellerCoin.Message);
                                }
                                //买方扣U
                                var subBuyerCoin = await ChangeWalletAmount(transaction, true, (long)order.BuyerUid, "USDT(ERC20)", -(Math.Round((decimal)order.TotalPrice, 4)), LfexCoinnModifyType.Lf_buy_Coin_Sub_Usdt, true, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round((decimal)order.TotalPrice, 4).ToString());
                                if (subBuyerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subBuyerCoin.Message);
                                }
                                //买方加币
                                var addBuyerCoin = await ChangeWalletAmount(transaction, true, (long)order.BuyerUid, model.CoinType, Math.Round(order.Amount.Value, 4), LfexCoinnModifyType.Lf_buy_Coin, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType);
                                if (addBuyerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, addBuyerCoin.Message);
                                }
                                //卖方加U
                                var subSellerUsdtCoin = await ChangeWalletAmount(transaction, true, userId, "USDT(ERC20)", Math.Round((decimal)order.TotalPrice, 4), LfexCoinnModifyType.Lf_Sell_Coin_Add_Usdt, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round((decimal)order.TotalPrice, 4).ToString());
                                if (subSellerUsdtCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subSellerUsdtCoin.Message);
                                }
                                //发放交易挖矿奖励
                                if (model.CoinType != "LF")
                                {
                                    var currentCoinPrice = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select `nowPrice` from `coin_type` where name='{model.CoinType}'", null, transaction);
                                    var lFCoinPrice = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select `nowPrice` from `coin_type` where name='LF'", null, transaction);
                                    if (lFCoinPrice != 0)
                                    {
                                        var reawrdCoinAmount = fee * currentCoinPrice * 0.5M / lFCoinPrice;
                                        var addRewardLFCoin = await ChangeWalletAmount(transaction, true, (long)order.BuyerUid, "LF", Math.Round(reawrdCoinAmount, 4), LfexCoinnModifyType.BuyCoinRewardCoin, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round(reawrdCoinAmount, 4).ToString());
                                    }
                                }
                                else
                                {
                                    var c1Num = Math.Round(TotalCandy, 4);
                                    var c2Num = Math.Round(order.Amount.Value, 4);
                                    var c1 = RedisCache.Publish("Lfex_Member_LFChange", JsonConvert.SerializeObject(new { sId = userId, sBalance = -c1Num, bId = order.BuyerUid, bBalance = c2Num }));
                                    if (c1 == 0)
                                    {
                                        SystemLog.Debug($"出售方法c1{c1}异常");
                                    }
                                    //系统手续费记录
                                    await ChangeWalletAmount(transaction, true, 1, model.CoinType, Math.Round(fee, 4), LfexCoinnModifyType.Lf_Change_Fee, false, Math.Round(fee, 4).ToString());
                                }
                                transaction.Commit();
                                result.Data = true;
                                return result;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Yoyo.Core.SystemLog.Debug($"系统错误请重试", ex);
                                return result.SetStatus(ErrorCode.SystemError, $"系统错误请重试{ex}");
                            }
                            finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                        }
                        #endregion
                        //fabi卖方 冻结其账户U
                        var res1 = await FrozenWalletAmount(transaction, true, userId, TotalCandy, "USDT(ERC20)");
                        if (res1.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res1.Message);
                        }

                        base.dbConnection.Execute($"update coin_trade set sellerUid = {userId},sellerAlipay='{userInfo.Alipay}',fee = {fee},dealTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',dealEndTime='{DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss")}', status = 2 where id = {model.OrderNum}", null, transaction);

                        //我的信息记录
                        var msg = $"你发布的{Math.Round((decimal)order.Amount.Value, 4)}{model.CoinType}买单已被接单，请到“{model.CoinType}”-“订单”-“交易中” 查看卖家支付宝，并按买单中显示的金额付款，上传付款截图";
                        base.dbConnection.Execute($"insert into notice_infos (userId, content, refId, type,title) values ({order.BuyerUid}, '{msg}', '{model.OrderNum}', '1','买{model.CoinType}通知')", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug("确认出售", ex);
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                if (model.Title != "bibi")
                {
                    //短信通知
                    await CommonSendToBuyer(buyerMobile);
                }
                base.dbConnection.Close();
                result.Data = true;
            }
            catch (System.Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"{userId}==>{model.GetJson()}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "入参非法[L]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
            return result;
        }

        /// <summary>
        /// 确认购买
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ConfirmBuy(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            if (!String.IsNullOrWhiteSpace(AppSetting.TradeTips))
            {
                return result.SetStatus(ErrorCode.InvalidData, AppSetting.TradeTips);
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                #region 基础验证
                if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "入参非法"); }
                if (userId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
                if (string.IsNullOrEmpty(model.TradePwd)) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空"); }
                if (string.IsNullOrEmpty(model.OrderNum)) { return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空"); }
                if (!ProcessSqlStr(model.OrderNum)) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }
                if (DateTime.Now.TimeOfDay > AppSetting.TradeEndTime.TimeOfDay || DateTime.Now.TimeOfDay < AppSetting.TradeStartTime.TimeOfDay)
                {
                    return result.SetStatus(ErrorCode.TimeNoOpen, $"交易时间为每天{AppSetting.TradeStartTime.ToString("HH:mm")}-{AppSetting.TradeEndTime.ToString("HH:mm")}");
                }
                #endregion

                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"ConfirmBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");
                var userInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>($"select * from `user` where id={userId}");
                if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空..."); }
                if (model.CoinType == "USDT(ERC20)")
                {
                    if (DateTime.Now.Hour >= 22 || DateTime.Now.Hour < 9)
                    {
                        return result.SetStatus(ErrorCode.TimeNoOpen, $"法币交易时间为每天9:00-22:00...");
                    }
                }
                #region  查询是否被封禁 (买方)
                //查询是否被封禁

                var banOrderCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(*) as count from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}'");
                if (banOrderCount != 0 && userInfo.Type != 1)
                {
                    //最近一次封禁时间
                    var lastBanTime = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(1);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }

                #endregion


                #region 会员验证

                //支付宝账号信息判断
                if (string.IsNullOrWhiteSpace(userInfo.Alipay)) { return result.SetStatus(ErrorCode.InvalidData, "您没有设置支付宝，买家无法打款给您。请完善信息后进行交易。"); }
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (reg.IsMatch(userInfo.Alipay)) { return result.SetStatus(ErrorCode.InvalidData, "您的支付宝账号异常，请完善后进行交易。"); }

                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
                if (userInfo.FreezeCandyNum < 0) { return result.SetStatus(ErrorCode.InvalidData, $"您的账户异常，请联系客服解决!"); }
                // if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 4 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号已被封禁 请联系管理员"); }

                //查订单是否存在
                var order = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select status,fee, buyerUid,sellerUid, amount, totalPrice from coin_trade where id ={model.OrderNum} and status=1");
                if (order == null) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单..."); }
                if (order.Status != 1) { return result.SetStatus(ErrorCode.SystemError, "此订单已经被别人抢单"); }
                #endregion

                #region 今日 是否存在未完成的订单
                StringBuilder QueryProgressOrderSql = new StringBuilder();
                QueryProgressOrderSql.Append($"SELECT count(id) AS count FROM coin_trade WHERE ( `sellerUid` = @UserId OR `buyerUid` = @UserId) and coinType='{model.CoinType}' AND `status` IN ( 1, 2, 3, 5 );");
                var isHasOrder = base.dbConnection.QueryFirstOrDefault<int>(QueryProgressOrderSql.ToString(), new { UserId = userId });
                if (isHasOrder > 0 && userInfo.Type != 1)
                {
                    return result.SetStatus(ErrorCode.HasValued, "您今天还有订单未完成，请先完成当前订单再交易[O]");
                }
                #endregion

                //买家和卖家不能相同
                if (order.SellerUid == userId) { return result.SetStatus(ErrorCode.SystemError, "自己无法卖给自己!"); }

                #region 匹配卖单
                StringBuilder DealSql = new StringBuilder();
                DealSql.Append("UPDATE `coin_trade` SET `buyerUid` = @Buyer, `buyerAlipay` = @BuyerAlipay, dealTime = @DealTime, dealEndTime = @DealEndTime, `status` = 2 ");
                DealSql.Append("WHERE id = @TradeId AND `status` = 1;");

                DynamicParameters DealParam = new DynamicParameters();
                DealParam.Add("TradeId", model.OrderNum, DbType.Int64);
                DealParam.Add("Buyer", userInfo.Id, DbType.Int64);
                DealParam.Add("BuyerAlipay", userInfo.Alipay, DbType.String);
                DealParam.Add("DealTime", DateTime.Now, DbType.DateTime);
                DealParam.Add("DealEndTime", DateTime.Now.AddMinutes(60), DbType.DateTime);

                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        if (model.Title == "bibi")
                        {
                            var TotalCandy = (decimal)(order.Amount + order.Fee);
                            try
                            {
                                #region 划转
                                base.dbConnection.Execute($"update coin_trade set buyerUid = {userId},buyerAlipay='{userInfo.Alipay}',dealTime='{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}',dealEndTime='{DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss")}', status = 4 where id = {model.OrderNum}", null, transaction);

                                //卖方扣币
                                var subSellerCoin = await ChangeWalletAmount(transaction, true, (long)order.SellerUid, model.CoinType, -Math.Round(TotalCandy, 4), LfexCoinnModifyType.Lf_Sell_Coin, true, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round((decimal)order.Fee, 4).ToString());
                                if (subSellerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subSellerCoin.Message);
                                }
                                //买方扣U
                                var subBuyerCoin = await ChangeWalletAmount(transaction, true, userId, "USDT(ERC20)", -(Math.Round((decimal)order.TotalPrice, 4)), LfexCoinnModifyType.Lf_buy_Coin_Sub_Usdt, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round((decimal)order.TotalPrice, 4).ToString());
                                if (subBuyerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subBuyerCoin.Message);
                                }
                                //买方加币
                                var addBuyerCoin = await ChangeWalletAmount(transaction, true, userId, model.CoinType, Math.Round(order.Amount.Value, 4), LfexCoinnModifyType.Lf_buy_Coin, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType);
                                if (addBuyerCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, addBuyerCoin.Message);
                                }
                                //卖方加U
                                var subSellerUsdtCoin = await ChangeWalletAmount(transaction, true, (long)order.SellerUid, "USDT(ERC20)", Math.Round((decimal)order.TotalPrice, 4), LfexCoinnModifyType.Lf_Sell_Coin_Add_Usdt, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round((decimal)order.TotalPrice, 4).ToString());
                                if (subSellerUsdtCoin.Code != 200)
                                {
                                    transaction.Rollback();
                                    return result.SetStatus(ErrorCode.SystemError, subSellerUsdtCoin.Message);
                                }
                                //发放交易挖矿奖励
                                if (model.CoinType != "LF")
                                {
                                    var currentCoinPrice = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select `nowPrice` from `coin_type` where name='{model.CoinType}'", null, transaction);
                                    var lFCoinPrice = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select `nowPrice` from `coin_type` where name='LF'", null, transaction);
                                    if (lFCoinPrice != 0)
                                    {
                                        var reawrdCoinAmount = (decimal)order.Fee * currentCoinPrice * 0.5M / lFCoinPrice;
                                        var addRewardLFCoin = await ChangeWalletAmount(transaction, true, userId, "LF", Math.Round(reawrdCoinAmount, 4), LfexCoinnModifyType.BuyCoinRewardCoin, false, Math.Round(order.Amount.Value, 4).ToString(), model.CoinType, Math.Round(reawrdCoinAmount, 4).ToString());
                                    }
                                }
                                else
                                {
                                    var c1Num = Math.Round(TotalCandy, 4);
                                    var c2Num = Math.Round(order.Amount.Value, 4);
                                    var c1 = RedisCache.Publish("Lfex_Member_LFChange", JsonConvert.SerializeObject(new { sId = order.SellerUid, sBalance = -c1Num, bId = userId, bBalance = c2Num }));
                                    if (c1 == 0)
                                    {
                                        SystemLog.Debug($"购买方法c1{c1}异常");
                                    }
                                    //系统手续费记录
                                    await ChangeWalletAmount(transaction, true, 1, model.CoinType, Math.Round(order.Fee.Value, 4), LfexCoinnModifyType.Lf_Change_Fee, false, Math.Round(order.Fee.Value, 4).ToString());
                                }
                                transaction.Commit();
                                result.Data = true;
                                return result;
                                #endregion
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                Yoyo.Core.SystemLog.Debug($"系统错误请重试", ex);
                                return result.SetStatus(ErrorCode.SystemError, $"系统错误请重试{ex}");
                            }
                            finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                        }
                        //fabi
                        int rows = dbConnection.Execute(DealSql.ToString(), DealParam, transaction);
                        if (rows < 1)
                        {
                            transaction.Rollback();
                            return result.SetStatus(ErrorCode.InvalidData, "匹配订单失败[R]");
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug("确认购买异常==>", ex);
                    }
                    finally
                    {
                        if (base.dbConnection.State == ConnectionState.Open) { dbConnection.Close(); }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                SystemLog.Debug($"{userId}==>{model.GetJson()}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "入参非法[L]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
            return result;
        }

        /// <summary>
        /// 通知买方 CommonSendToBuyer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendToBuyer(string mobile)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();

            StringContent content = new StringContent(new { mobile = mobile, temp_id = "184449" }.GetJson());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/messages", content);
            String res = await response.Content.ReadAsStringAsync();
            result.Data = res.GetModel<MsgDto>();
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await base.dbConnection.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 通知卖方 CommonSendToSeller
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendToSeller(string mobile)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();

            StringContent content = new StringContent(new { mobile = mobile, temp_id = "184450" }.GetJson());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/messages", content);
            String res = await response.Content.ReadAsStringAsync();
            result.Data = res.GetModel<MsgDto>();
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await base.dbConnection.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        public MyResult<domain.models.yoyoDto.CoinTradeExt> GetTradeTotal(string coinType)
        {
            MyResult<domain.models.yoyoDto.CoinTradeExt> result = new MyResult<domain.models.yoyoDto.CoinTradeExt>();

            String TradeTotalCoinKey = $"System:TradeTotalCoin_{coinType}";
            result.Data = RedisCache.Get<domain.models.yoyoDto.CoinTradeExt>(TradeTotalCoinKey);
            if (result.Data == null)
            {
                //查币种
                var rowId = base.dbConnection.QueryFirstOrDefault<int>($"select id from `coin_type` where name='{coinType}'");
                if (rowId <= 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "币种类型非法");
                }
                #region 计算面板信息
                StringBuilder QueryCoinSql = new StringBuilder();
                QueryCoinSql.Append("SELECT SUM( `amount` ) AS todayTradeAmount, AVG( `price` ) AS todayAvgPrice, MAX( `price` ) AS todayMaxPrice ");
                QueryCoinSql.Append($"FROM `coin_trade` WHERE `status` = 4 and coinType='{coinType}' AND DATE( `dealTime` ) = DATE(NOW());");
                domain.models.yoyoDto.CoinTradeExt coinTrade = base.dbConnection.QueryFirstOrDefault<domain.models.yoyoDto.CoinTradeExt>(QueryCoinSql.ToString());
                //买单量
                Int32 buyCount = base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT ifnull(SUM(amount),0) AS todayAmount FROM `coin_trade` WHERE `status` > 0 and coinType='{coinType}' AND `status` < 4;");
                //查询今天是否有记录
                Int32 isHasRecord = base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT id FROM coin_trade_ext WHERE DATE(ctime) = DATE(NOW()) and type='{coinType}';");
                var SysMaxPrice = 10M;
                var SysMinPrice = 0.01M;
                if (coinType == "LF")
                {
                    SysMaxPrice = 3;
                    SysMinPrice = 0.7M;
                }
                else if (coinType == "YB")
                {
                    SysMaxPrice = 2;
                    SysMinPrice = 0.5M;
                }
                else if (coinType == "糖果")
                {
                    SysMaxPrice = 1;
                    SysMinPrice = 0.07M;
                }
                else if (coinType == "USDT(ERC20)")
                {
                    SysMaxPrice = 8;
                    SysMinPrice = 6;
                }
                if (isHasRecord != 0)
                {
                    StringBuilder UpdateCoinSql = new StringBuilder();
                    UpdateCoinSql.Append("UPDATE `coin_trade_ext` SET ");
                    UpdateCoinSql.Append("`todayTradeAmount` = @TodayTradeAmount, ");
                    UpdateCoinSql.Append("`todayMaxPrice` = @TodayMaxPrice, ");
                    UpdateCoinSql.Append("`todayAvgPrice` = @TodayAvgPrice, ");
                    UpdateCoinSql.Append("`sysMinPrice` = @SysMinPrice, ");
                    UpdateCoinSql.Append("`sysMaxPrice` = @SysMaxPrice, ");
                    UpdateCoinSql.Append("`todayAmount` = @BuyCount, ");
                    UpdateCoinSql.Append("`upRate` = @UpRate WHERE DATE(ctime) = DATE(NOW()) AND `type`=@Type;");

                    DynamicParameters UpdateCoinParam = new DynamicParameters();
                    UpdateCoinParam.Add("TodayTradeAmount", coinTrade.TodayTradeAmount, DbType.Decimal);
                    UpdateCoinParam.Add("TodayMaxPrice", coinTrade.TodayMaxPrice, DbType.Decimal);
                    UpdateCoinParam.Add("TodayAvgPrice", coinTrade.TodayAvgPrice, DbType.Decimal);
                    UpdateCoinParam.Add("BuyCount", buyCount, DbType.Int32);
                    UpdateCoinParam.Add("UpRate", 0.001M, DbType.Decimal);
                    UpdateCoinParam.Add("Type", coinType, DbType.String);
                    UpdateCoinParam.Add("SysMinPrice", SysMinPrice, DbType.Decimal);
                    UpdateCoinParam.Add("SysMaxPrice", SysMaxPrice, DbType.Decimal);

                    base.dbConnection.Execute(UpdateCoinSql.ToString(), UpdateCoinParam);
                }
                else
                {
                    StringBuilder InsertCoinSql = new StringBuilder();
                    InsertCoinSql.Append("INSERT INTO `coin_trade_ext` ( `todayTradeAmount`, `todayAvgPrice`, `todayAmount`, `todayMaxPrice`,`type`,`sysMinPrice`,`sysMaxPrice` ) ");
                    InsertCoinSql.Append("VALUES (@TodayTradeAmount, @TodayAvgPrice, @BuyCount, @TodayMaxPrice,@Type,@SysMinPrice,@SysMaxPrice);");

                    DynamicParameters InsertCoinParam = new DynamicParameters();
                    InsertCoinParam.Add("TodayTradeAmount", coinTrade.TodayTradeAmount, DbType.Decimal);
                    InsertCoinParam.Add("TodayAvgPrice", coinTrade.TodayAvgPrice, DbType.Decimal);
                    InsertCoinParam.Add("TodayMaxPrice", coinTrade.TodayMaxPrice, DbType.Decimal);
                    InsertCoinParam.Add("BuyCount", buyCount, DbType.Int32);
                    InsertCoinParam.Add("Type", coinType, DbType.String);
                    InsertCoinParam.Add("SysMinPrice", SysMinPrice, DbType.Decimal);
                    InsertCoinParam.Add("SysMaxPrice", SysMaxPrice, DbType.Decimal);
                    base.dbConnection.Execute(InsertCoinSql.ToString(), InsertCoinParam);
                }
                #endregion

                #region 获取面板信息
                List<domain.models.yoyoDto.CoinTradeExt> tradeCoins = base.dbConnection.Query<domain.models.yoyoDto.CoinTradeExt>($"SELECT * FROM `coin_trade_ext` where type='{coinType}' ORDER BY id DESC LIMIT 2;").ToList();
                domain.models.yoyoDto.CoinTradeExt coinTradeExt = new domain.models.yoyoDto.CoinTradeExt();

                if (tradeCoins.Count == 2)
                {
                    coinTradeExt = new domain.models.yoyoDto.CoinTradeExt
                    {
                        LastAvgPrice = tradeCoins[0].TodayAvgPrice,
                        LastMaxPrice = tradeCoins[1].TodayMaxPrice,
                        LastTradeAmount = tradeCoins[1].TodayTradeAmount,
                        SysMaxPrice = tradeCoins[0].SysMaxPrice,
                        SysMinPrice = tradeCoins[0].SysMinPrice,
                        TodayAmount = tradeCoins[0].TodayAmount,
                        TodayAvgPrice = tradeCoins[0].TodayAvgPrice,
                        TodayMaxPrice = tradeCoins[0].TodayMaxPrice,
                        TodayTradeAmount = tradeCoins[0].TodayTradeAmount,
                        UpRate = tradeCoins[0].UpRate
                    };
                }

                if (tradeCoins.Count == 1)
                {
                    coinTradeExt = new domain.models.yoyoDto.CoinTradeExt
                    {
                        LastAvgPrice = tradeCoins[0].TodayAvgPrice,
                        LastMaxPrice = tradeCoins[0].LastMaxPrice,
                        SysMaxPrice = tradeCoins[0].SysMaxPrice,
                        SysMinPrice = tradeCoins[0].SysMinPrice,
                        TodayAmount = tradeCoins[0].TodayAmount,
                        TodayAvgPrice = tradeCoins[0].TodayAvgPrice,
                        TodayMaxPrice = tradeCoins[0].TodayMaxPrice,
                        TodayTradeAmount = tradeCoins[0].TodayTradeAmount,
                        LastTradeAmount = tradeCoins[0].LastTradeAmount,
                        UpRate = tradeCoins[0].UpRate
                    };
                }
                #endregion
                coinTradeExt.SellMaxPrice = AppSetting.SellMaxPrice;
                coinTradeExt.SellMinPrice = AppSetting.SellMinPrice;
                //USDT(ERC20)--特殊处理
                if (coinType.Equals("USDT(ERC20)"))
                {
                    coinTradeExt.SellMaxPrice = 8M;
                    coinTradeExt.SellMinPrice = 6M;
                    coinTradeExt.SysMaxPrice = 8M;
                    coinTradeExt.SysMinPrice = 6M;
                }
                coinTradeExt.LastAvgPrice = Math.Round(coinTradeExt.LastAvgPrice, 2);
                coinTradeExt.TodayAvgPrice = Math.Round(coinTradeExt.TodayAvgPrice, 2);
                RedisCache.Set(TradeTotalCoinKey, coinTradeExt, 60);
                result.Data = coinTradeExt;
            }
            return result;
        }

        /// <summary>
        /// 我的交易订单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<List<TradeListReturnDto>>> MyTradeList(MyTradeListDto model, int userId)
        {
            MyResult<List<TradeListReturnDto>> result = new MyResult<List<TradeListReturnDto>>();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            model.PageIndex = model.PageIndex < 1 ? 1 : model.PageIndex;
            #region 拼接SQL 查询条件
            StringBuilder SqlStr = new StringBuilder();
            SqlStr.Append("FROM ");
            SqlStr.Append("`coin_trade` AS ct ");
            SqlStr.Append("LEFT JOIN `user` AS s ON ct.`sellerUid` = s.id ");
            SqlStr.Append("LEFT JOIN `user` AS b ON ct.`buyerUid` = b.id ");
            SqlStr.Append("LEFT JOIN `authentication_infos` ais ON ct.`sellerUid` = ais.`userId` ");
            SqlStr.Append("LEFT JOIN `authentication_infos` aib ON ct.`buyerUid` = aib.`userId` ");
            SqlStr.Append($"WHERE 1 = 1 and ct.coinType='{model.CoinType}' ");

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);

            if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
            {
                if (model.Status == 1)
                {
                    QueryParam.Add("Sale", "BUY", DbType.String);
                    SqlStr.Append("AND ct.trendSide=@Sale ");

                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND ct.buyerUid=@UserId AND ct.status=1 ");
                }
                else if (model.Status == 2)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.buyerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status IN (2,3,5) ");
                }
                else if (model.Status == 3)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.buyerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status=4 ");
                }
            }
            else
            {
                if (model.Status == 1)
                {
                    QueryParam.Add("Sale", "SELL", DbType.String);
                    SqlStr.Append("AND ct.trendSide=@Sale ");
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND ct.sellerUid=@UserId AND ct.status=1 ");
                }
                else if (model.Status == 2)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.sellerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status IN (2,3,5) ");
                }
                else if (model.Status == 3)
                {
                    QueryParam.Add("UserId", userId, DbType.Int32);
                    SqlStr.Append("AND (ct.sellerUid=@UserId OR ct.sellerUid=@UserId) AND ct.status=4 ");
                }
            }
            SqlStr.Append("ORDER BY ct.utime DESC ");
            #endregion

            #region 查询总数
            StringBuilder CountSqlStr = new StringBuilder();
            CountSqlStr.Append("SELECT COUNT(ct.`id`) ");
            CountSqlStr.Append(SqlStr);
            CountSqlStr.Append(";");
            #endregion

            #region 拼接查询字段
            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT ct.`id` AS `Id`, ");
            QuerySqlStr.Append("ais.`trueName` AS `SellerTrueName`, ");
            QuerySqlStr.Append("aib.`trueName` AS `BuyerTrueName`, ");
            QuerySqlStr.Append("ct.`tradeNumber` AS `TradeNumber`, ");
            QuerySqlStr.Append("ct.`buyerUid` AS `BuyerUid`, ");
            QuerySqlStr.Append("b.`mobile` AS `BuyerMobile`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, b.`avatarUrl`) AS `BuyerAvatarUrl`, ");
            QuerySqlStr.Append("ct.`buyerAlipay` AS `BuyerAlipay`, ");
            QuerySqlStr.Append("ct.`sellerUid` AS `SellerUid`, ");
            QuerySqlStr.Append("s.`mobile` AS `SellerMobile`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, s.`alipayPic`) AS `AlipayPic`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, s.`avatarUrl`) AS `SellerAvatarUrl`, ");
            QuerySqlStr.Append("ct.`sellerAlipay` AS `SellerAlipay`, ");
            QuerySqlStr.Append("CONCAT(@CosUrl, ct.`pictureUrl`) AS `PictureUrl`, ");
            QuerySqlStr.Append("ct.`amount` AS `Amount`, ");
            QuerySqlStr.Append("ct.`price` AS `Price`, ");
            QuerySqlStr.Append("ct.`totalPrice` AS `TotalPrice`, ");
            QuerySqlStr.Append("ct.`dealTime` AS `DealTime`, ");
            QuerySqlStr.Append("ct.`dealEndTime` AS `DealEndTime`, ");
            QuerySqlStr.Append("ct.`paidTime` AS `PaidTime`, ");
            QuerySqlStr.Append("ct.`paidEndTime` AS `PaidEndTime`, ");
            QuerySqlStr.Append("ct.`status` AS `Status` ");
            #endregion

            QuerySqlStr.Append(SqlStr);
            QuerySqlStr.Append("LIMIT @PageIndex, @PageSize;");
            QueryParam.Add("PageIndex", (model.PageIndex - 1) * model.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", model.PageSize, DbType.Int32);

            try
            {
                IEnumerable<TradeListReturnDto> coinTradeInfo = await base.dbConnection.QueryAsync<TradeListReturnDto>(QuerySqlStr.ToString(), QueryParam);

                result.Data = new List<TradeListReturnDto>();
                foreach (var item in coinTradeInfo)
                {
                    TradeListReturnDto tradeInfo = item;
                    if (!String.IsNullOrWhiteSpace(tradeInfo.BuyerMobile))
                    {
                        tradeInfo.BuyerMobile = Regex.Replace(item.BuyerMobile, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
                    }
                    if (!String.IsNullOrWhiteSpace(item.SellerMobile))
                    {
                        tradeInfo.SellerMobile = Regex.Replace(item.SellerMobile, "(\\d{3})\\d{4}(\\d{4})", "$1****$2");
                    }
                    result.Data.Add(tradeInfo);
                }
                result.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>(CountSqlStr.ToString(), QueryParam);
                result.PageCount = result.RecordCount / model.PageSize;
            }
            catch (Exception ex)
            {
                result.RecordCount = 0;
                result.PageCount = 0;
                result.Data = new List<TradeListReturnDto>();
                LogUtil<TradeService>.Error(ex, "我的交易订单");
            }
            return result;
        }

        public async Task<MyResult<object>> Paid(PaidDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"Paid:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                //查订单是否存在
                var orderInfo = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select status,sellerUid from coin_trade where id ={model.OrderNum} and status=2");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }
                if (orderInfo.Status != 2)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }
                //卖方手机号
                var sellerMobile = base.dbConnection.QueryFirstOrDefault<string>($"select mobile from user where id={orderInfo.SellerUid}");
                //更新订单状态 写记录 发短信
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        //卖方消息通知
                        var msg = $"买家已标记付款，请到 法币-订单-交易中 验证交易详情，确认支付宝是否到账，无误后请尽快释放";
                        var sql = $"insert into notice_infos (userId, content, refId, type,title) values ({orderInfo.SellerUid}, '{msg}', '{model.OrderNum}', '1','卖通知')";
                        base.dbConnection.Execute(sql, null, transaction);
                        var paidTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        var paidEndTime = DateTime.Now.AddMinutes(60).ToString("yyyy-MM-dd HH:mm:ss");
                        //更新订单信息
                        base.dbConnection.Execute($"update coin_trade set pictureUrl='{model.PicUrl}',paidTime='{paidTime}',paidEndTime='{paidEndTime}',status = 3 where id = {model.OrderNum}", null, transaction);
                        transaction.Commit();
                        result.Data = new { PaidTime = paidTime, PaidEndTime = paidEndTime };
                    }
                    catch (System.Exception ex)
                    {
                        LogUtil<TradeService>.Error(ex.Message);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();
                //短信通知
                await CommonSendToSeller(sellerMobile);

                return result;
            }
            catch (Exception ex)
            {
                LogUtil<TradeService>.Error(ex, "Paid订单支付发生错误。");
                return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        public MyResult<object> ForcePaidCoin()
        {
            MyResult<object> result = new MyResult<object>();

            return result;
        }

        /// <summary>
        /// 确认到账 发放
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> PaidCoin(PaidDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单号不能为空");
            }
            if (!ProcessSqlStr(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (string.IsNullOrEmpty(model.TradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"PaidCoin:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                //用户信息
                var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
                if (userInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "用户信息不能为空...");
                }
                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd)
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误");
                }
                //订单信息
                var orderInfo = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"SELECT `status`, amount, fee,coinType, buyerUid, sellerUid, trendSide FROM coin_trade WHERE id = {model.OrderNum} AND `status` =3;");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常 请联系管理员");
                }
                if (orderInfo.Status != 3)
                {
                    return result.SetStatus(ErrorCode.SystemError, "订单状态异常");
                }


                #region 计算正常手续费
                decimal NormalFee = 0;

                NormalFee = orderInfo.Amount.Value * 0.01M;
                #endregion

                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        //果皮扣除记录
                        var systemUserId = 1;
                        var TotalCandy = orderInfo.Amount + orderInfo.Fee;

                        //减掉卖家用户的冻结账户中的冻结余额并添加流水
                        var res1 = await ChangeWalletAmount(transaction, true, userId, "USDT(ERC20)", -(decimal)(orderInfo.Amount.Value + orderInfo.Fee.Value), LfexCoinnModifyType.Lf_Sell_Coin, true, Math.Round((decimal)orderInfo.Amount, 4).ToString(), orderInfo.CoinType, Math.Round((decimal)orderInfo.Fee, 4).ToString());
                        if (res1.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res1.Message);
                        }

                        //增加买家账户中的余额并添加流水
                        var res2 = await ChangeWalletAmount(transaction, true, (long)orderInfo.BuyerUid, "USDT(ERC20)", (decimal)orderInfo.Amount.Value, LfexCoinnModifyType.Lf_buy_Coin, false, Math.Round((decimal)orderInfo.Amount, 4).ToString(), orderInfo.CoinType);
                        if (res2.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res2.Message);
                        }
                        //将手续费划入系统
                        var res3 = await ChangeWalletAmount(transaction, true, systemUserId, "USDT(ERC20)", (decimal)orderInfo.Fee.Value, LfexCoinnModifyType.Lf_Sell_Sys_Fee, false, orderInfo.TradeNumber, orderInfo.Fee.ToString());
                        if (res3.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res3.Message);
                        }
                        //更新订单信息
                        base.dbConnection.Execute($"update coin_trade set status=4 where id = {model.OrderNum}", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        LogUtil<TradeService>.Error(ex.Message);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();
                result.Data = true;
                return result;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 发布买单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> StartBuy(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Sign Error");
            }
            if (model == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "数据类型非法...");
            }
            if (string.IsNullOrEmpty(model.TradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空");
            }
            if (model.Amount <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "数量不能小于0");
            }
            if (model.Price <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能低于0");
            }
            if (model.CoinType == "USDT(ERC20)")
            {
                if (model.Price < 6)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "法币挂单价最低为6");
                }
            }
            if (model.CoinType == "YB")
            {
                if (model.Price < 0.2)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "YB挂单价最低为0.2USDT");
                }
            }
            if (model.CoinType == "糖果")
            {
                if (model.Price < 0.06)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "今日糖果挂单价最低为0.06USDT");
                }
            }
            model.Price = Math.Round(model.Price, 4);
            #region 系统单价判定

            StringBuilder QueryPriceSql = new StringBuilder();
            QueryPriceSql.Append($"SELECT sysMinPrice, sysMaxPrice FROM coin_trade_ext where type='{model.CoinType}' ORDER BY id DESC");
            domain.models.yoyoDto.CoinTradeExt PriceLimit = await dbConnection.QueryFirstOrDefaultAsync<domain.models.yoyoDto.CoinTradeExt>(QueryPriceSql.ToString());
            if (model.Price < PriceLimit.SysMinPrice.ToDouble())
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能低于" + PriceLimit.SysMinPrice.ToString("0.00"));
            }
            if (model.Price > PriceLimit.SysMaxPrice.ToDouble())
            {
                return result.SetStatus(ErrorCode.InvalidData, "价格不能高于" + PriceLimit.SysMaxPrice.ToString("0.00"));
            }
            #endregion

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"StartBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 查询是否交易封禁
                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");
                //查询是否被封禁
                var selectCoinSql = $"select count(*) as count from coin_trade where buyerBan=1 and buyerUid = { userId } and coinType='{model.CoinType}' and dealTime> '{startDayForMonth}' and dealTime<'{lastDayForMonth}'";
                var banOrderCount = base.dbConnection.QueryFirstOrDefault<int>(selectCoinSql);
                User userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
                if (banOrderCount != 0 && userInfo.Type != 1)
                {
                    //最近一次封禁时间
                    var lastBanTime = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(1);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }
                #endregion

                #region 会员信息验证
                //查询钱包余额
                var coinBalance = base.dbConnection.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_wallet` where `CoinType`='USDT(ERC20)' and userId={userId}");
                if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
                // if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }

                #endregion

                #region 交易数量限制
                Int32 coinCount = base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE buyerUid = @UserId and coinType='{model.CoinType}' AND `status` != 0;", new { UserId = userId });
                coinCount += base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid = @UserId and coinType='{model.CoinType}' AND `status` != 0;", new { UserId = userId });
                if (coinCount == 0 && model.Amount > 5) { return result.SetStatus(ErrorCode.InvalidData, "首次交易订单数量不得大于5"); }
                if (model.Amount > 1000) { return result.SetStatus(ErrorCode.InvalidData, "交易订单数量不得大于1000"); }

                StringBuilder QueryIngSql = new StringBuilder();
                QueryIngSql.Append($"SELECT COUNT(*) AS count FROM coin_trade WHERE buyerUid = @UserId AND trendSide = 'BUY' and coinType='{model.CoinType}' AND `status` != 0 AND `status` != 4 AND `status` != 6 ");
                QueryIngSql.Append($"UNION SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid = @UserId AND trendSide = 'BUY' and coinType='{model.CoinType}' AND `status` != 0 AND `status` != 4 AND `status` != 6;");

                var orderIng = base.dbConnection.QueryFirstOrDefault<int>(QueryIngSql.ToString(), new { UserId = userId });
                if (orderIng >= 1 && userInfo.Type != 1) { return result.SetStatus(ErrorCode.InvalidData, "当前有未完成订单"); }
                #endregion
                //订单信息
                var orderNum = Gen.NewGuid20();
                if (model.CoinType == "YB")
                {
                    orderNum = Gen.NewGuidN();
                }
                var totalPrice = model.Amount * model.Price;
                //bibi
                if (model.Title == "bibi")
                {
                    if (coinBalance < (decimal)totalPrice)
                    {
                        return result.SetStatus(ErrorCode.SystemError, $"钱包USDT余额{coinBalance}/不足,请去法币交易大厅购买USDT");
                    }
                    var frozenInfo = await FrozenWalletAmount(null, false, userId, (decimal)totalPrice, "USDT(ERC20)");
                    if (frozenInfo.Code != 200)
                    {
                        SystemLog.Debug($"发布买单冻结糖果=={frozenInfo.Message}");
                        return result.SetStatus(ErrorCode.SystemError, frozenInfo.Message);
                    }
                }
                //发布订单
                var insertSql = $"insert into coin_trade(tradeNumber,buyerUid,buyerAlipay,amount,price,totalPrice,fee,trendSide,status,coinType)values('{orderNum}',{userId},'{userInfo.Alipay}',{model.Amount},{model.Price},{totalPrice},0,'BUY',1,'{model.CoinType}');SELECT @@IDENTITY";
                var res = base.dbConnection.ExecuteScalar<long>(insertSql);
                if (res == 0)
                {
                    return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
                }

                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                LogUtil<TradeService>.Error(ex, "发布买单失败");
                return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 发布卖单
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> StartSell(TradeDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            if (model == null) { return result.SetStatus(ErrorCode.InvalidData, "数据非法..."); }
            if (string.IsNullOrEmpty(model.TradePwd)) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码不能为空"); }
            if (model.Amount <= 0) { return result.SetStatus(ErrorCode.InvalidData, "数量不能小于0"); }
            if (model.Price <= 0) { return result.SetStatus(ErrorCode.InvalidData, "价格不能低于0"); }
            model.Price = Math.Round(model.Price, 4);

            #region 系统单价判定
            if (model.Price.ToDecimal() > AppSetting.SellMaxPrice)
            {
                return result.SetStatus(ErrorCode.InvalidData, $"价格不能高于{AppSetting.SellMaxPrice.ToString("0.00")}");
            }
            if (model.Price.ToDecimal() < AppSetting.SellMinPrice)
            {
                return result.SetStatus(ErrorCode.InvalidData, $"价格不能低于{AppSetting.SellMinPrice.ToString("0.00")}");
            }
            #endregion

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"StartBuy:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                //===糖果转入后24小时内禁止交易===//
                if (model.CoinType == "糖果")
                {
                    var inTime = base.dbConnection.QueryFirstOrDefault<DateTime>($"select uawr.`ModifyTime` from (select AccountId from `user_account_wallet` where `coinType`='{model.CoinType}' and userId={userId}) a left join `user_account_wallet_record` uawr on a.`AccountId`=uawr.`AccountId` where `ModifyType`=24 order by uawr.`RecordId` desc limit 1");
                    if (inTime.ToString("yyyy-MM-dd") != "0001-01-01")
                    {
                        if (DateTime.Now < inTime.AddHours(24))
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"转入{model.CoinType}24小时内，禁止交易，交易解禁时间为:{inTime.AddHours(24).ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }

                }
                //===fabi 禁止价格低于6====//
                if (model.CoinType == "USDT(ERC20)")
                {
                    if (model.Price < 6)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "法币挂单价最低为6");
                    }
                    if (model.Price > 8)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "法币挂单价最高为8");
                    }
                }
                if (model.CoinType == "糖果")
                {
                    if (model.Price < 0.048)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "今日糖果挂单价最低为0.048USDT");
                    }
                }
                #region 否被封禁
                var lastDayForMonth = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-01")).AddMonths(1).AddDays(-1).ToString("yyyy-MM-dd");
                var startDayForMonth = DateTime.Now.ToString("yyyy-MM-01");
                //查询是否被封禁
                var selectCoinSql = $"select count(*) as count from coin_trade where buyerBan=1 and coinType='{model.CoinType}' and buyerUid = { userId } and dealTime> '{startDayForMonth}' and dealTime<'{lastDayForMonth}'";
                var banOrderCount = base.dbConnection.QueryFirstOrDefault<int>(selectCoinSql);
                var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
                if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
                //非商家禁止挂卖
                if (userInfo.Type != 1)
                {
                    if (model.CoinType == "USDT(ERC20)" || model.CoinType == "LF")
                    {
                        if (model.Amount > 100)
                        {
                            return new MyResult<object>(500, "请去买单出售...");
                        }

                    }
                    else
                    {
                        return new MyResult<object>(500, "请去买单出售...");
                    }

                }
                if (banOrderCount != 0 && userInfo.Type != 1)
                {
                    //最近一次封禁时间
                    var lastBanTime = base.dbConnection.QueryFirstOrDefault<CoinTrade>($"select entryOrderTime from coin_trade where buyerBan=1 and buyerUid={userId} and dealTime>'{startDayForMonth}' and dealTime<'{lastDayForMonth}' order by id desc limit 1");
                    if (lastBanTime != null)
                    {
                        DateTime beginTime = DateTime.Now;
                        beginTime = ((DateTime)lastBanTime.EntryOrderTime).AddDays(1);
                        if (DateTime.Now < beginTime)
                        {
                            return result.SetStatus(ErrorCode.Forbidden, $"您当前已经被封禁交易，解封时间为：{beginTime.ToString("yyyy-MM-dd HH:mm")}");
                        }
                    }
                }
                #endregion

                #region 验证账户  是否存在异常
                //查询钱包余额
                var coinBalance = base.dbConnection.QueryFirstOrDefault<decimal>($"select `Balance` from `user_account_wallet` where `CoinType`='{model.CoinType}' and userId={userId}");
                if (SecurityUtil.MD5(model.TradePwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
                // if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
                if (string.IsNullOrEmpty(userInfo.Alipay))
                {
                    return result.SetStatus(ErrorCode.NoAuth, "交易前请设置支付宝账号...");
                }
                if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }
                if (string.IsNullOrWhiteSpace(userInfo.Alipay)) { return result.SetStatus(ErrorCode.InvalidData, "您没有设置支付宝，买家无法打款给您。请完善信息后进行交易。"); }
                Regex reg = new Regex(@"[\u4e00-\u9fa5]");
                if (reg.IsMatch(userInfo.Alipay)) { return result.SetStatus(ErrorCode.InvalidData, "您的支付宝账号异常，请完善后进行交易。"); }

                #region 新的计算手续费
                String UserLevel = userInfo.Level.ToLower();
                Decimal TradeFee = 0.00M;
                if (UserLevel.Equals("lv0")) { return result.SetStatus(ErrorCode.InvalidData, "LV0禁止交易"); }
                decimal SellRate = 0.01M;
                if (model.CoinType == "LF")
                {
                    if (userInfo.Level.ToLower().Equals("lv1"))
                    {
                        SellRate = 0.5M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv2"))
                    {
                        SellRate = 0.35M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv3"))
                    {
                        SellRate = 0.3M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv4"))
                    {
                        SellRate = 0.28M;
                    }
                    else if (userInfo.Level.ToLower().Equals("lv5"))
                    {
                        SellRate = 0.25M;
                    }
                    else
                    {
                        SellRate = 0.25M;
                    }
                }
                TradeFee = model.Amount * SellRate;
                #endregion

                Decimal TotalCandy = model.Amount + TradeFee;

                if (coinBalance < TotalCandy)
                {
                    return result.SetStatus(ErrorCode.InvalidData, $"您当前{model.CoinType}为{coinBalance},不足以出售{model.Amount}个{model.CoinType}");
                }
                #endregion

                #region 交易限制
                Int32 coinCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE buyerUid = @UserId and coinType='{model.CoinType}' AND `status` != 0;", new { UserId = userId });
                coinCount += base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid = @UserId and coinType='{model.CoinType}' AND `status` != 0;", new { UserId = userId });
                if (coinCount == 0 && model.Amount > 5) { return result.SetStatus(ErrorCode.InvalidData, "首次交易订单数量不得大于5"); }

                if (model.Amount > 100000) { return result.SetStatus(ErrorCode.InvalidData, "交易订单数量不得大于10w"); }
                Int32 orderIng = base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT COUNT(id) AS count FROM coin_trade WHERE sellerUid ={userId} AND trendSide = 'SELL' and coinType='{model.CoinType}' AND `status` != 4 AND `status` != 0 AND `status` != 6;");
                if (orderIng >= 1 && userInfo.Type != 1) { return result.SetStatus(ErrorCode.InvalidData, "当前有未完成订单"); }

                Int32 orderCount = base.dbConnection.QueryFirstOrDefault<Int32>($"SELECT COUNT(*) AS count FROM coin_trade WHERE sellerUid ={userId} AND `status` = 4 and coinType='{model.CoinType}' AND TO_DAYS(dealTime) = to_days(NOW());");

                orderCount = orderIng + orderCount;
                if (orderCount >= 3 && userInfo.Type == 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "出售次数受限，每日只可交易三次");
                }
                #endregion

                #region 卖单发布
                var orderNum = Gen.NewGuid20();
                if (model.CoinType == "YB")
                {
                    orderNum = Gen.NewGuidN();
                }
                var totalPrice = model.Amount * model.Price;

                StringBuilder InsertOrderSql = new StringBuilder();
                InsertOrderSql.Append("INSERT INTO coin_trade (tradeNumber, sellerUid, sellerAlipay, amount, price, totalPrice, fee, trendSide, `status`,`coinType`) VALUES ");
                InsertOrderSql.Append($"( @TradeNum, @SellerUid, @SellerAlipay, @SellAmount, @UnitPrice, @TotalPrice, @TradeFee, 'SELL', 1,@coinType);SELECT @@IDENTITY");
                DynamicParameters OrderParam = new DynamicParameters();
                OrderParam.Add("TradeNum", orderNum, DbType.String);
                OrderParam.Add("SellerUid", userInfo.Id, DbType.Int64);
                OrderParam.Add("SellerAlipay", userInfo.Alipay, DbType.String);
                OrderParam.Add("SellAmount", model.Amount, DbType.Int32);
                OrderParam.Add("UnitPrice", model.Price, DbType.Decimal);
                OrderParam.Add("TotalPrice", totalPrice, DbType.Decimal);
                OrderParam.Add("TradeFee", TradeFee, DbType.Decimal);
                OrderParam.Add("coinType", model.CoinType, DbType.String);

                Int64 TradeId;
                var rowId = await FrozenWalletAmount(null, false, userId, TotalCandy, model.CoinType);
                if (rowId.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
                }
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {

                        TradeId = dbConnection.ExecuteScalar<Int64>(InsertOrderSql.ToString(), OrderParam, transaction);
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Yoyo.Core.SystemLog.Debug("发布卖单异常==>>", ex);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
                    }
                    finally
                    {
                        if (null != CacheLock) { CacheLock.Unlock(); }
                    }

                }
                #endregion

                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("发布买单失败", ex);
                return result.SetStatus(ErrorCode.SystemError, "发布订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }


        }

        /// <summary>
        /// Coin钱包账户余额变更 common
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeWalletAmount(IDbTransaction OutTran, bool isUserOutTransaction, long userId, string coinType, decimal Amount, LfexCoinnModifyType modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            UserAccountWallet UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} AND `CoinType`='{coinType}' LIMIT 1";
                if (isUserOutTransaction)
                {
                    UserAccount = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql, null, OutTran);
                }
                else
                {
                    UserAccount = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
                }
                if (UserAccount == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
                if (Amount < 0)
                {
                    if (useFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[F]"); }
                    }
                    else
                    {
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[B]"); }
                    }
                }
                #endregion

                AccountId = UserAccount.AccountId;
                Field = Amount > 0 ? "Revenue" : "Expenses";

                EditSQl = $"UPDATE `{AccountTableName}` SET `Balance`=`Balance`+{Amount},{(useFrozen ? $"`Frozen`=`Frozen`+{Amount}," : "")}`{Field}`=`{Field}`+{Math.Abs(Amount)},`ModifyTime`=NOW() WHERE `AccountId`={AccountId} {(useFrozen ? $"AND (`Frozen`+{Amount})>=0;" : $"AND(`Balance`-`Frozen`+{Amount}) >= 0;")}";

                PostChangeSql = $"IFNULL((SELECT `PostChange` FROM `{RecordTableName}` WHERE `AccountId`={AccountId} ORDER BY `RecordId` DESC LIMIT 1),0)";
                StringBuilder TempRecordSql = new StringBuilder($"INSERT INTO `{RecordTableName}` ");
                TempRecordSql.Append("( `AccountId`, `PreChange`, `Incurred`, `PostChange`, `ModifyType`, `ModifyDesc`, `ModifyTime` ) ");
                TempRecordSql.Append($"SELECT {AccountId} AS `AccountId`, ");
                TempRecordSql.Append($"{PostChangeSql} AS `PreChange`, ");
                TempRecordSql.Append($"{Amount} AS `Incurred`, ");
                TempRecordSql.Append($"{PostChangeSql}+{Amount} AS `PostChange`, ");
                TempRecordSql.Append($"{(int)modifyType} AS `ModifyType`, ");
                TempRecordSql.Append($"'{String.Join(',', Desc)}' AS `ModifyDesc`, ");
                TempRecordSql.Append($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' AS`ModifyTime`");
                RecordSql = TempRecordSql.ToString();

                #region 修改账务
                if (base.dbConnection.State == ConnectionState.Closed) { base.dbConnection.Open(); }
                if (isUserOutTransaction)
                {
                    IDbTransaction Tran = OutTran;
                    try
                    {
                        Int32 EditRow = base.dbConnection.Execute(EditSQl, null, Tran);
                        Int32 RecordId = base.dbConnection.Execute(RecordSql, null, Tran);
                        if (EditRow == RecordId && EditRow == 1)
                        {
                            if (!isUserOutTransaction)
                            {
                                Tran.Commit();
                            }
                            return new MyResult { Data = true };
                        }
                        Tran.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally
                    {
                        if (!isUserOutTransaction)
                        {
                            if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); }
                        }
                    }
                }
                else
                {
                    using (IDbTransaction Tran = base.dbConnection.BeginTransaction())
                    {
                        try
                        {
                            Int32 EditRow = base.dbConnection.Execute(EditSQl, null, Tran);
                            Int32 RecordId = base.dbConnection.Execute(RecordSql, null, Tran);
                            if (EditRow == RecordId && EditRow == 1)
                            {
                                if (!isUserOutTransaction)
                                {
                                    Tran.Commit();
                                }
                                return new MyResult { Data = true };
                            }
                            Tran.Rollback();
                            return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                        }
                        catch (Exception ex)
                        {
                            Tran.Rollback();
                            Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                            return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                        }
                        finally
                        {
                            if (!isUserOutTransaction)
                            {
                                if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); }
                            }
                        }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 钱包账户余额冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> FrozenWalletAmount(IDbTransaction OutTran, bool isUserOutTransaction, long userId, decimal Amount, string coinType)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={userId} AND `CoinType`='{coinType}' AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                if (isUserOutTransaction)
                {
                    int Row = await base.dbConnection.ExecuteAsync(UpdateSql, null, OutTran);
                    if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                    result.Data = true;
                    return result;
                }
                else
                {
                    int Row = await base.dbConnection.ExecuteAsync(UpdateSql);
                    if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                    result.Data = true;
                    return result;
                }

            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"账户余额冻结操作发生错误,\r\n{UpdateSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 交易订单列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<List<CoinTradeDto>>> TradeList(TradeReqDto model)
        {
            MyResult<List<CoinTradeDto>> result = new MyResult<List<CoinTradeDto>>();
            model.PageIndex = model.PageIndex < 1 ? 1 : model.PageIndex;

            if (!ProcessSqlStr(model.Type)) { return result.SetStatus(ErrorCode.Unauthorized, "你涉嫌非法入侵已经被我方风控系统抓捕稍后会有官方人员和你取得联系"); }

            try
            {
                #region 拼接SQL
                StringBuilder QuerySql = new StringBuilder();
                StringBuilder CountSql = new StringBuilder();
                //左连接
                var lefjoin = $"";
                if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    lefjoin = $" left join user u on u.id=t.buyerUid";
                }
                else
                {
                    lefjoin = $" left join user u on u.id=t.sellerUid";
                }
                CountSql.Append("SELECT COUNT(`id`) FROM `coin_trade` WHERE `status` = 1 ");
                QuerySql.Append("SELECT t.`id`, t.`tradeNumber`,u.`name`,u.`type` uType, t.`buyerUid`, t.`sellerUid`, t.`price`, t.`totalPrice`, t.`amount`, t.`entryOrderTime`, ");
                QuerySql.Append($"(SELECT COUNT(id) FROM coin_trade WHERE buyerUid = t.buyerUid AND `coinType`='{model.CoinType}' AND status = 4 AND dealTime BETWEEN DATE_SUB(NOW(),INTERVAL 30 DAY) AND NOW()) AS Dishonesty ");
                QuerySql.Append($"FROM `coin_trade` AS t{lefjoin} WHERE t.`status` = 1 ");

                if (String.IsNullOrWhiteSpace(model.Sale) || model.Sale.Equals("BUY", StringComparison.OrdinalIgnoreCase))
                {
                    CountSql.Append("AND `trendSide` = 'BUY' ");
                    QuerySql.Append("AND `trendSide` = 'BUY' ");

                    if (!string.IsNullOrEmpty(model.SearchText))
                    {
                        CountSql.Append("AND buyerAlipay = @SearchText ");
                        QuerySql.Append("AND buyerAlipay = @SearchText ");
                    }
                    if (!string.IsNullOrEmpty(model.CoinType))
                    {
                        CountSql.Append("AND coinType = @CoinType ");
                        QuerySql.Append("AND coinType = @CoinType ");
                    }
                    switch (model.Type.ToLower())
                    {
                        case "system":
                            // CountSql.Append("AND `buyerUid` = 1 ");

                            // QuerySql.Append("AND `buyerUid` = 1 ");
                            QuerySql.Append("ORDER BY price");
                            break;
                        case "amount":
                            // CountSql.Append("AND `buyerUid` != 1 ");

                            // QuerySql.Append("AND `buyerUid` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        case "price":
                            // CountSql.Append("AND `buyerUid` != 1 ");

                            // QuerySql.Append("AND `buyerUid` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    CountSql.Append("AND `trendSide` = 'SELL' ");
                    QuerySql.Append("AND `trendSide` = 'SELL' ");

                    if (!string.IsNullOrEmpty(model.SearchText))
                    {
                        CountSql.Append("AND sellerAlipay = @SearchText ");
                        QuerySql.Append("AND sellerAlipay = @SearchText ");
                    }
                    if (!string.IsNullOrEmpty(model.CoinType))
                    {
                        CountSql.Append("AND coinType = @CoinType ");
                        QuerySql.Append("AND coinType = @CoinType ");
                    }
                    switch (model.Type.ToLower())
                    {
                        case "system":
                            CountSql.Append("AND `sellerAlipay` = 1 ");

                            QuerySql.Append("AND `sellerAlipay` = 1 ");
                            QuerySql.Append("ORDER BY price");
                            break;
                        case "amount":
                            CountSql.Append("AND `sellerAlipay` != 1 ");

                            QuerySql.Append("AND `sellerAlipay` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        case "price":
                            CountSql.Append("AND `sellerAlipay` != 1 ");

                            QuerySql.Append("AND `sellerAlipay` != 1 ");
                            QuerySql.Append("ORDER BY ");
                            QuerySql.Append(model.Type);
                            break;
                        default:
                            break;
                    }
                }

                if (model.Order.Equals("asc", StringComparison.OrdinalIgnoreCase))
                {
                    QuerySql.Append(" ASC ");
                }
                else { QuerySql.Append(" DESC "); }

                QuerySql.Append("LIMIT @PageIndex, @PageSize;");

                DynamicParameters Param = new DynamicParameters();
                Param.Add("SearchText", model.SearchText, DbType.String);
                Param.Add("CoinType", model.CoinType, DbType.String);
                Param.Add("PageIndex", (model.PageIndex - 1) * model.PageSize, DbType.Int32);
                Param.Add("PageSize", model.PageSize, DbType.Int32);

                #endregion
                IEnumerable<CoinTradeDto> TradeList = await base.dbConnection.QueryAsync<CoinTradeDto>(QuerySql.ToString(), Param);
                result.Data = TradeList.ToList();
                result.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>(CountSql.ToString(), Param);
                result.PageCount = (result.RecordCount + model.PageSize - 1) / model.PageSize;
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug(model.GetJson(), ex);
                result.Data = new List<CoinTradeDto>();
                return result;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        private static bool ProcessSqlStr(string Str)
        {
            string SqlStr;
            SqlStr = " |,|=|'|and|exec|insert|select|delete|update|count|*|chr|mid|master|truncate|char|declare";
            Str = Str.ToLower();
            bool ReturnValue = true;
            if (String.IsNullOrWhiteSpace(Str)) { return ReturnValue; }
            try
            {
                if (Str != "")
                {
                    string[] anySqlStr = SqlStr.Split('|');
                    foreach (string ss in anySqlStr)
                    {
                        if (Str.IndexOf(ss) >= 0)
                        {
                            ReturnValue = false;
                        }
                    }
                }
            }
            catch
            {
                ReturnValue = false;
            }
            return ReturnValue;
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<ListModel<TradeOrder>>> QueryTradeOrder(QueryTradeOrder query)
        {
            MyResult<ListModel<TradeOrder>> Rult = new MyResult<ListModel<TradeOrder>>();

            DynamicParameters QueryParam = new DynamicParameters();

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT o.id,o.coinType,o.tradeNumber, o.buyerUid, o.buyerAlipay, o.sellerUid, au.trueName, o.sellerAlipay, ");
            QuerySql.Append("o.amount, o.price, o.totalPrice, o.paidTime, o.pictureUrl, o.dealTime, o.`status`, o.appealTime, o.dealTime, o.buyerBan, o.sellerBan ");
            QuerySql.Append("FROM coin_trade AS o LEFT JOIN authentication_infos AS au ON o.sellerUid = au.userId WHERE 1 = 1 ");

            StringBuilder QueryCuntSql = new StringBuilder();
            QueryCuntSql.Append("SELECT COUNT(o.id) FROM coin_trade AS o LEFT JOIN authentication_infos AS au ON o.sellerUid = au.userId WHERE 1 = 1 ");

            if (query.Status != TradeState.All)
            {
                QuerySql.Append("AND o.status = @Status ");
                QueryCuntSql.Append("AND o.status = @Status ");
            }

            #region 类型
            switch (query.Type)
            {
                case "buyer":
                    if (!String.IsNullOrWhiteSpace(query.Alipay))
                    {
                        QueryParam.Add("Alipay", query.Alipay, DbType.String);
                        QuerySql.Append("AND o.buyerAlipay = @Alipay ");
                        QueryCuntSql.Append("AND o.buyerAlipay = @Alipay; ");
                    }
                    if (!String.IsNullOrWhiteSpace(query.Mobile))
                    {
                        QueryParam.Add("Mobile", query.Mobile, DbType.String);
                        QuerySql.Append("AND o.buyerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1) ");
                        QueryCuntSql.Append("AND o.buyerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1); ");
                    }
                    break;
                case "seller":
                    if (!String.IsNullOrWhiteSpace(query.Alipay))
                    {
                        QueryParam.Add("Alipay", query.Alipay, DbType.String);
                        QuerySql.Append("AND o.sellerAlipay = @Alipay ");
                        QueryCuntSql.Append("AND o.sellerAlipay = @Alipay; ");
                    }
                    if (!String.IsNullOrWhiteSpace(query.Mobile))
                    {
                        QueryParam.Add("Mobile", query.Mobile, DbType.String);
                        QuerySql.Append("AND o.sellerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1) ");
                        QueryCuntSql.Append("AND o.sellerUid = (SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1); ");
                    }
                    break;
                default:
                    break;
            }
            #endregion
            QueryParam.Add("Status", (Int32)query.Status, DbType.Int32);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);

            Rult.RecordCount = await dbConnection.QueryFirstOrDefaultAsync<Int32>(QueryCuntSql.ToString(), QueryParam);
            QuerySql.Append("ORDER BY o.id DESC LIMIT @PageIndex, @PageSize;");

            IEnumerable<CoinTradeModel> TradeOrders = await dbConnection.QueryAsync<CoinTradeModel>(QuerySql.ToString(), QueryParam);

            Rult.PageCount = Rult.RecordCount / query.PageSize;
            Rult.Data = new ListModel<TradeOrder>()
            {
                List = new List<TradeOrder>()
            };
            foreach (var item in TradeOrders)
            {
                TradeOrder order = new TradeOrder()
                {
                    Id = item.Id,
                    OrderId = item.TradeNumber,
                    BuyerUid = item.BuyerUid ?? 0,
                    BuyerAlipay = item.BuyerAlipay,
                    SellerUid = item.SellerUid ?? 0,
                    SellerAlipay = item.SellerAlipay,
                    TrueName = item.TrueName,
                    UnitPrice = item.Price ?? 0,
                    SellCount = item.Amount ?? 0,
                    TotalPrice = item.TotalPrice ?? 0,
                    TradeFee = item.Fee ?? 0,
                    ConfirmTime = item.DealTime,
                    PayTime = item.PaidTime,
                    CoinType = item.CoinType,
                    TradeState = (TradeState)item.Status,
                    AppealTime = item.AppealTime
                };
                if (item.BuyerBan != item.SellerBan)
                {
                    order.TimeOutUser = item.BuyerBan > item.SellerBan ? "买家超时" : "卖家超时";
                }
                if (!string.IsNullOrWhiteSpace(item.PictureUrl)) { order.PayPic = $"https://file.yoyoba.cn/{item.PictureUrl}"; }
                Rult.Data.List.Add(order);
            }

            return Rult;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CloseTradeOrder(TradeOrder order)
        {
            MyResult<object> result = new MyResult<object>();
            Boolean IsFail = true;
            if (order.TradeState == TradeState.Completed) { return result.SetStatus(ErrorCode.SystemError, "此订单交易已完成"); }
            if (order.TradeState == TradeState.Normal || order.TradeState == TradeState.Cancelled) { return result.SetStatus(ErrorCode.SystemError, "此订单无需操作"); }

            StringBuilder CloseTradeSql = new StringBuilder();
            CloseTradeSql.Append("UPDATE `coin_trade` SET `status` = 0,`buyerBan`=1,`utime` = NOW() WHERE `id` = @Id;");
            DynamicParameters CloseTradeParam = new DynamicParameters();
            CloseTradeParam.Add("Id", order.Id, DbType.Int64);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    //查订单
                    var orderInfo = await base.dbConnection.QueryFirstOrDefaultAsync<CoinTrade>($"select * from coin_trade where id={order.Id}", null, transaction);
                    if (orderInfo != null)
                    {
                        base.dbConnection.Execute(CloseTradeSql.ToString(), CloseTradeParam, transaction);
                        await FrozenWalletAmount(transaction, true, order.SellerUid, -(decimal)(orderInfo.Amount + orderInfo.Fee), "USDT(ERC20)");
                        transaction.Commit();
                        IsFail = false;
                    }
                    else
                    {
                        IsFail = true;
                    }
                }
                catch (Exception ex)
                {
                    LogUtil<SystemService>.Warn(ex.Message);
                    transaction.Rollback();
                }
            }
            base.dbConnection.Close();

            if (IsFail) { return result.SetStatus(ErrorCode.SystemError, "关闭订单失败"); }
            result.Data = true;
            return result;

        }

        /// <summary>
        /// 恢复订单至
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ResumeTradeOrder(TradeOrder order)
        {
            MyResult<object> result = new MyResult<object>();

            CoinTrade TradeOrder = base.dbConnection.QueryFirstOrDefault<CoinTrade>("SELECT * FROM coin_trade WHERE id = @Id;", new { Id = order.Id });
            order.TradeState = (TradeState)TradeOrder.Status;
            if (order.TradeState == TradeState.Completed) { return result.SetStatus(ErrorCode.SystemError, "此订单交易已完成"); }
            if (order.TradeState == TradeState.WaitPay) { return result.SetStatus(ErrorCode.SystemError, "此订单未支付"); }
            if (order.TradeState == TradeState.Normal || order.TradeState == TradeState.Cancelled || order.TradeState == TradeState.AlreadyPay) { return result.SetStatus(ErrorCode.SystemError, "此订单无需操作"); }

            StringBuilder ResumeTradeSql = new StringBuilder();
            DynamicParameters ResumeTradeParam = new DynamicParameters();
            Int32 rows = 0;
            if (order.TradeState == TradeState.Appeal)
            {
                ResumeTradeSql.Append("UPDATE `coin_trade` SET `status` = 3, `paidEndTime` = @PaidEndTime WHERE `id` = @Id;");
                ResumeTradeParam.Add("Id", order.Id, DbType.Int64);
                ResumeTradeParam.Add("PaidEndTime", DateTime.Now.AddDays(1), DbType.DateTime);
                rows = await base.dbConnection.ExecuteAsync(ResumeTradeSql.ToString(), ResumeTradeParam);
            }
            TradeOrder.BuyerBan = TradeOrder.BuyerBan ?? 0;
            TradeOrder.SellerBan = TradeOrder.SellerBan ?? 0;
            if (order.TradeState == TradeState.TimeOut && TradeOrder.SellerBan == 1) { return result.SetStatus(ErrorCode.SystemError, "此订单不可恢复"); }
            if (order.TradeState == TradeState.TimeOut && TradeOrder.BuyerBan == 1)
            {
                ResumeTradeParam.Add("Id", order.Id, DbType.Int64);
                ResumeTradeParam.Add("UserId", TradeOrder.SellerUid, DbType.Int64);
                ResumeTradeParam.Add("PaidEndTime", DateTime.Now.AddDays(1), DbType.DateTime);
                ResumeTradeParam.Add("FreezeCandy", TradeOrder.Amount + TradeOrder.Fee, DbType.Decimal);
                rows = await base.dbConnection.ExecuteAsync("UPDATE `user` SET freezeCandyNum = freezeCandyNum + @FreezeCandy, candyNum = candyNum - @FreezeCandy WHERE id = @UserId And candyNum > @FreezeCandy;", ResumeTradeParam);
                if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "恢复失败,卖家不足"); }
                rows = await base.dbConnection.ExecuteAsync("UPDATE `coin_trade` SET `status` = 2, `paidEndTime` = @PaidEndTime, `dealEndTime` = @PaidEndTime WHERE `id` = @Id;", ResumeTradeParam);
            }
            if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "恢复失败"); }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 封禁买家
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> BanBuyer(TradeOrder order)
        {
            MyResult<object> result = new MyResult<object>();

            StringBuilder BaneSql = new StringBuilder();
            BaneSql.Append("UPDATE `user` SET `status` = 2, `passwordSalt` = @Reason WHERE `id` = @UserId;");
            DynamicParameters ResumeTradeParam = new DynamicParameters();
            ResumeTradeParam.Add("UserId", order.BuyerUid, DbType.Int64);
            ResumeTradeParam.Add("Reason", order.AppealReason ?? "", DbType.String);

            Int32 rows = await base.dbConnection.ExecuteAsync(BaneSql.ToString(), ResumeTradeParam);
            if (rows < 1) { return result.SetStatus(ErrorCode.SystemError, "封禁失败"); }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 解除订单超时封禁
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Unblock(TradeOrder order)
        {
            MyResult<object> rult = new MyResult<object>();
            DynamicParameters UpdateParam = new DynamicParameters();
            UpdateParam.Add("Id", order.Id);
            Int32 rows = await base.dbConnection.ExecuteAsync("UPDATE coin_trade SET buyerBan = 0, sellerBan = 0 WHERE id = @Id;", UpdateParam);

            if (rows > 0)
            {
                rult.Data = rows;
                return rult;
            }
            return rult.SetStatus(ErrorCode.InvalidData, "解除失败");
        }

        /// <summary>
        /// 查询申诉
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<List<TradeAppeals>>> ViewAppeal(TradeOrder order)
        {
            MyResult<List<TradeAppeals>> Rult = new MyResult<List<TradeAppeals>>();

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("OrderId", order.Id, DbType.Int64);
            IEnumerable<TradeAppeals> appeals = await base.dbConnection.QueryAsync<TradeAppeals>("SELECT id, orderId, picUrl, description, createdAt, `status` FROM	appeals WHERE orderId = @OrderId;", QueryParam);

            Rult.Data = new List<TradeAppeals>();

            foreach (var item in appeals)
            {
                item.PicUrl = item.PicUrl ?? "";
                item.PicUrl = $"https://file.yoyoba.cn/{item.PicUrl}";
                Rult.Data.Add(item);
            }

            return Rult;
        }

        /// <summary>
        /// 交易面板
        /// </summary>
        /// <param name="coinType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<domain.models.yoyoDto.CoinTradeModelDto> CoinTradeTotal(string coinType, int userId)
        {
            MyResult<CoinTradeModelDto> result = new MyResult<CoinTradeModelDto>();

            String TradeTotalCoinKey = $"System:CoinTradeTotal_{coinType}_{userId}";
            result.Data = RedisCache.Get<CoinTradeModelDto>(TradeTotalCoinKey);
            if (result.Data == null)
            {
                #region 获取面板信息
                List<CoinTradeModelDto> tradeCoins = base.dbConnection.Query<CoinTradeModelDto>($"SELECT * FROM `coin_trade_ext` where type='{coinType}' ORDER BY id DESC LIMIT 1;").ToList();

                CoinTradeModelDto coinTradeExt = new CoinTradeModelDto();

                //可以用币
                var canUserUSDTSql = $"select IFNULL(Balance,0) from `user_account_wallet` where CoinType='USDT(ERC20)' and UserId={userId}";
                var canUserCoinSql = $"select IFNULL(Balance,0) from `user_account_wallet` where CoinType='{coinType}' and UserId={userId}";
                var canUserCoin = base.dbConnection.QueryFirstOrDefault<decimal>(canUserCoinSql);
                var canUserUSDT = base.dbConnection.QueryFirstOrDefault<decimal>(canUserUSDTSql);


                if (tradeCoins.Count == 1)
                {
                    coinTradeExt = new CoinTradeModelDto
                    {
                        SysMaxPrice = tradeCoins[0].SysMaxPrice,
                        SysMinPrice = tradeCoins[0].SysMinPrice,
                        UpRate = tradeCoins[0].UpRate,
                        CanUserCoin = canUserCoin,
                        CanUserUSDT = canUserUSDT
                    };
                }
                #endregion
                RedisCache.Set(TradeTotalCoinKey, coinTradeExt, 60);
                result.Data = coinTradeExt;
            }
            return result;
        }

        public async Task<MyResult<object>> MyTradeListWt(string coinType, int userId)
        {
            MyResult result = new MyResult();
            var sql = $"select id,`ctime`,`trendSide`,`price`,`Amount` from `coin_trade` where `coinType`='{coinType}' and status=1 and (`buyerUid`={userId} or `sellerUid`={userId}) order by id desc";
            result.Data = await base.dbConnection.QueryAsync(sql);
            return result;
        }
    }
}