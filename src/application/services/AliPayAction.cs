using application.services.bases;
using domain.configs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using domain.models.yoyoDto;
using System.Data;
using Yoyo.Core;
using application.Utils;
using domain.repository;
using CSRedis;
using domain.enums;
using Microsoft.Extensions.Options;
using domain.lfexentitys;
using infrastructure.utils;

namespace application.services
{
    public class AliPayAction : BaseServiceLfex, IAliPayAction
    {
        private readonly IAlipay AlipaySub;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting AppSetting;
        private readonly IUserWalletAccountService UserWallet;
        public AliPayAction(IAlipay alipay, IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient, IOptionsMonitor<Models.AppSetting> monitor, IUserWalletAccountService userWallet) : base(connectionStringList)
        {
            this.AlipaySub = alipay;
            this.RedisCache = redisClient;
            this.AppSetting = monitor.CurrentValue;
            this.UserWallet = userWallet;
        }

        /// <summary>
        /// 二次认证的异步通知
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> AuthAliPay(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await base.dbConnection.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `yoyo_pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                base.dbConnection.Open();
                using (IDbTransaction transaction = base.dbConnection.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = base.dbConnection.Execute($"UPDATE `yoyo_pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        var UserRow = base.dbConnection.Execute($"UPDATE `user` SET `alipayUid`='{ChannelUid}' WHERE `id`={OrderInfo.UserId}", null, transaction);
                        transaction.Commit();
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        /// <summary>
        /// 修改支付宝的异步通知
        /// </summary>
        /// <param name="TradeNo"></param>
        /// <returns></returns>
        public async Task<String> ChangeAliPay(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await base.dbConnection.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `yoyo_pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                base.dbConnection.Open();
                using (IDbTransaction transaction = base.dbConnection.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = base.dbConnection.Execute($"UPDATE `yoyo_pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        var UserRow = base.dbConnection.Execute($"UPDATE `user` SET `alipay`='{OrderInfo.Custom}' WHERE `id`={OrderInfo.UserId}", null, transaction);
                        transaction.Commit();
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        public async Task<String> CashRecharge(String TradeNo)
        {
            if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
            var OrderInfo = await base.dbConnection.QueryFirstOrDefaultAsync<PayInfo>($"SELECT * FROM `yoyo_pay_record` WHERE `PayId`={TradeNo} LIMIT 1");
            if (OrderInfo == null) { return "fail"; }
            if (OrderInfo.PayStatus != PayStatus.UN_PAID) { return "fail"; }
            try
            {
                base.dbConnection.Open();
                using (IDbTransaction transaction = base.dbConnection.BeginTransaction())
                {
                    try
                    {
                        var ChannelUid = String.IsNullOrWhiteSpace(PayRult.Result.BuyerUserId) ? String.Empty : PayRult.Result.BuyerUserId;
                        var PayRow = base.dbConnection.Execute($"UPDATE `yoyo_pay_record` SET `PayStatus`={(int)PayStatus.PAID},`ChannelUID`='{ChannelUid}',`ModifyTime`=NOW() WHERE `PayId`={TradeNo}", null, transaction);
                        transaction.Commit();
                        await UserWallet.ChangeWalletAmount(OrderInfo.UserId, OrderInfo.Amount, AccountModifyType.CASH_RECHARGE, false, OrderInfo.PayId.ToString());
                        return "success";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        SystemLog.Error($"修改支付宝时发生错误,订单号{TradeNo}", ex);
                        return "fail";
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                }
            }
            catch { return "fail"; }
        }

        /// <summary>
        /// 修改支付宝账号
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Alipay"></param>
        /// <param name="PayPwd"></param>
        /// <param name="AlipayPic"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> ModifyAlipay(Int64 UserId, String Alipay, String PayPwd, string AlipayPic)
        {
            MyResult result = new MyResult();

            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            User userInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>($"select * from user where id={UserId}");
            if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
            if (SecurityUtil.MD5(PayPwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
            if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }

            #region 拼装SQL 并扣款
            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", UserId, DbType.Int64);
            DeductParams.Add("Alipay", Alipay, DbType.String);
            DeductParams.Add("AlipayPic", AlipayPic, DbType.String);
            DeductSql.Append("UPDATE `user` SET alipay = @Alipay,alipayPic=@AlipayPic ");
            DeductSql.Append("WHERE id = @UserId");

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug(new { UserId, PayPwd, Alipay }, ex);
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open) { dbConnection.Close(); }
                }
            }
            #endregion
            return result;
        }

        /// <summary>
        /// 生成支付链接的方法
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Amount"></param>
        /// <param name="action"></param>
        /// <param name="Custom"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CreatePayUrl(int UserId, Decimal Amount, ActionType action, String Custom = "")
        {
            MyResult result = new MyResult();
            if (UserId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"CreatePayUrl:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                decimal Fee = Math.Ceiling(Amount * 0.006M * 100.00M) * 0.01M;
                PayInfo info = new PayInfo
                {
                    UserId = UserId,
                    Channel = PayChannel.AliPay,
                    Currency = Currency.Rmb,
                    Custom = String.IsNullOrWhiteSpace(Custom) ? String.Empty : Custom,
                    Amount = Amount,
                    Fee = Fee,
                    ActionType = action,
                    ChannelUID = String.Empty,
                    CreateTime = DateTime.Now,
                    ModifyTime = null,
                    PayStatus = PayStatus.UN_PAID,
                };

                String PayId = base.dbConnection.ExecuteScalar<String>("INSERT INTO `yoyo_pay_record` (`UserId`, `Channel`, `Currency`, `Amount`, `Fee`, `ActionType`, `Custom`, `PayStatus`, `ChannelUID`, `CreateTime`) VALUES (@UserId, @Channel, @Currency,@Amount, @Fee,@ActionType, @Custom, @PayStatus, @ChannelUID, NOW());SELECT @@IDENTITY", info);
                if (String.IsNullOrWhiteSpace(PayId)) { return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败"); }
                String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit() { OutTradeNo = PayId, TotalAmount = Amount.ToString("0.00"), Subject = action.GetDescription(), NotifyUrl = AppSetting.AlipayNotify, TimeOutExpress = "15m", PassbackParams = action.ToString() });
                result.Data = AppUrl;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
            return result;
        }
    }
}
