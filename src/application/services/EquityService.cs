using CSRedis;
using Dapper;
using domain.configs;
using domain.lfexentitys;
using domain.enums;
using domain.models;
using domain.models.equity;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Yoyo.Core;

namespace application.services
{
    /// <summary>
    /// 股权
    /// </summary>
    public class EquityService : bases.BaseServiceLfex, IEquityService
    {
        private readonly String AccountTableName = "user_account_equity";
        private readonly String RecordTableName = "user_account_equity_record";
        private readonly String CacheLockKey = "EquityAccount:";

        private readonly CSRedisClient RedisCache;
        private readonly Models.EquityConfig EquityConf;
        public EquityService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient, IOptionsMonitor<Models.EquityConfig> monitor) : base(connectionStringList)
        {
            RedisCache = redisClient;
            EquityConf = monitor.CurrentValue;
        }

        /// <summary>
        /// 股权信息
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<EquityInfo>> EquityPage(long UserId)
        {
            MyResult<EquityInfo> result = new MyResult<EquityInfo>()
            {
                Data = new EquityInfo()
            };
            result.Data.CandyLimit = EquityConf.CandyLimit;
            result.Data.Rules = EquityConf.Rules;
            result.Data.UnitPrice = EquityConf.UnitPrice;
            User UserInfo = await dbConnection.QueryFirstOrDefaultAsync<User>("SELECT candyNum, candyP, mobile FROM `user` WHERE id = @UserId;", new { UserId });
            if (UserInfo != null)
            {
                result.Data.Mobile = UserInfo.Mobile ?? String.Empty;
                result.Data.Candy = UserInfo.CandyNum ?? 0;
                result.Data.Peel = UserInfo.CandyP;
            }
            var AuthInfo = dbConnection.QueryFirstOrDefault<domain.models.yoyoDto.AuthDto>("SELECT trueName, idNum FROM authentication_infos WHERE userId = @UserId", new { UserId });
            if (AuthInfo != null)
            {
                result.Data.TrueName = AuthInfo?.TrueName ?? String.Empty;
                result.Data.IDCardNum = AuthInfo?.IdNum ?? String.Empty;
            }
            var EquityAccount = await EquityInfo(UserId);
            if (EquityAccount != null && EquityAccount.Success)
            {
                result.Data.TotalShares = (Int32)EquityAccount.Data.Balance;
                result.Data.Convertible = (Int32)EquityAccount.Data.Balance - (Int32)EquityAccount.Data.Frozen;
            }
            return result;
        }

        /// <summary>
        /// 股权兑换
        /// </summary>
        /// <param name="exchange"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ExchangeEquity(EquityExchange exchange)
        {
            MyResult<Object> result = new MyResult<object>();
            if (EquityConf.SubscribeTotal == 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "本期股权已兑完");
            }

            Boolean IsSuccess = false;

            String CacheLock = $"Equity:Exchange{exchange.UserId}";

            if (RedisCache.Exists(CacheLock))
            {
                return result.SetStatus(ErrorCode.InvalidData, "您操作太快了~");
            }
            else
            {
                RedisCache.Set(CacheLock, exchange.Shares, 5);
            }

            #region 基础验证
            if (exchange.Shares < EquityConf.LimitShares)
            {
                return result.SetStatus(ErrorCode.ErrorSign, $"股权最低{EquityConf.LimitShares}股起兑");
            }
            Decimal CandyTotal = exchange.Shares * EquityConf.UnitPrice;
            Decimal PeelTotal = exchange.Shares * EquityConf.UnitPrice;
            if (exchange.UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
            await InitWalletAccount(exchange.UserId); //初始化账户

            Int32 EquityTotal = base.dbConnection.QueryFirstOrDefault<Int32>("SELECT SUM(Balance) FROM user_account_equity;");
            if (EquityTotal >= EquityConf.SubscribeTotal) { return result.SetStatus(ErrorCode.InvalidData, "本日股权已兑完,每天早点来哦~"); }
            if ((EquityConf.SubscribeTotal - EquityTotal) < exchange.Shares)
            {
                return result.SetStatus(ErrorCode.InvalidData, $"本日可兑股权，仅剩{EquityConf.SubscribeTotal - EquityTotal}份~");
            }
            var UserInfo = base.dbConnection.QueryFirstOrDefault<User>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = exchange.UserId });
            if (!UserInfo.TradePwd.Equals(SecurityUtil.MD5(exchange.PayPwd))) { return result.SetStatus(ErrorCode.InvalidData, "支付密码错误"); }
            if (UserInfo.CandyNum < CandyTotal) { return result.SetStatus(ErrorCode.InvalidData, "您的糖果不足"); }
            if (UserInfo.CandyP < PeelTotal) { return result.SetStatus(ErrorCode.InvalidData, "您的果皮不足"); }
            #endregion

            #region 拼装SQL 并扣款
            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", exchange.UserId, DbType.Int64);
            DeductParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            DeductParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 1
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", exchange.UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"股权认购: {exchange.Shares.ToString()}份", DbType.String);
            CandyRecordParams.Add("Source", 87, DbType.Int32);

            //写入 果皮扣除记录 1
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", exchange.UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"股权认购: {exchange.Shares.ToString()}份", DbType.String);
            PeelRecordParams.Add("Source", 87, DbType.Int32);

            try
            {
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Int32 Rows = 0;
                        Rows += dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                        Rows += dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                        Rows += dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                        if (Rows != 3) { throw new Exception("扣款失败[S]"); }
                        transaction.Commit();
                        IsSuccess = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug(exchange.GetJson(), ex);
                    }
                }
            }
            finally
            {
                if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); }
            }
            #endregion

            if (IsSuccess)
            {
                var rult = await ChangeWalletAmount(exchange.UserId, exchange.Shares, EquityModifyType.EQUITY_SUBSCRIBE, false, exchange.Shares.ToString());
                if (rult == null || !rult.Success) { return rult; }
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "股权兑换失败");
        }

        /// <summary>
        /// 股权转让
        /// </summary>
        /// <param name="transfer"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> TransferEquity(EquityTransfer transfer)
        {
            MyResult<Object> result = new MyResult<object>();
            String CacheLock = $"Equity:Transfer{transfer.UserId}";

            if (RedisCache.Exists(CacheLock))
            {
                return result.SetStatus(ErrorCode.InvalidData, "您操作太快了~");
            }
            else
            {
                RedisCache.Set(CacheLock, transfer.Shares, 5);
            }

            #region 基础验证
            if (transfer.Shares < 1) { result.SetStatus(ErrorCode.InvalidData, "请求参数有误"); }
            Decimal CandyTotal = transfer.Shares * EquityConf.UnitPrice * EquityConf.TransferFee;
            Decimal PeelTotal = transfer.Shares * EquityConf.UnitPrice * EquityConf.TransferFee;
            if (transfer.UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
            User UserInfo = base.dbConnection.QueryFirstOrDefault<User>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = transfer.UserId });
            if (UserInfo == null) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆后,重试"); }
            if (!UserInfo.TradePwd.Equals(SecurityUtil.MD5(transfer.PayPwd))) { return result.SetStatus(ErrorCode.InvalidData, "支付密码错误"); }
            if (UserInfo.CandyNum < CandyTotal) { return result.SetStatus(ErrorCode.InvalidData, "您的糖果不足"); }
            if (UserInfo.CandyP < PeelTotal) { return result.SetStatus(ErrorCode.InvalidData, "您的果皮不足"); }

            User ToUserInfo = base.dbConnection.QueryFirstOrDefault<User>("SELECT * FROM `user` WHERE mobile = @Mobile;", new { Mobile = transfer.Mobile });
            if (ToUserInfo == null || ToUserInfo.Status != 0 || ToUserInfo.AuditState != 2) { return result.SetStatus(ErrorCode.InvalidData, "转让账户异常"); }
            if (transfer.UserId == ToUserInfo.Id) { return result.SetStatus(ErrorCode.InvalidData, "自己不能转让给自己哦~"); }
            await InitWalletAccount(ToUserInfo.Id); //初始化账户
            #endregion

            #region 股权转让
            var rult = await ChangeWalletAmount(transfer.UserId, -transfer.Shares, EquityModifyType.EQUITY_ROLLOUT, false, transfer.Shares.ToString(), transfer.Mobile);
            if (rult == null || !rult.Success) { return rult; }
            rult = await ChangeWalletAmount(ToUserInfo.Id, transfer.Shares, EquityModifyType.EQUITY_TRANSFER_INTO, false, transfer.Shares.ToString());
            if (rult == null || !rult.Success) { return rult; }
            #endregion

            #region 拼装SQL 并扣款
            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", transfer.UserId, DbType.Int64);
            DeductParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            DeductParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 1
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", transfer.UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", CandyTotal, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"股权转让: {transfer.Shares.ToString()}份", DbType.String);
            CandyRecordParams.Add("Source", 86, DbType.Int32);

            //写入 果皮扣除记录 1
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", transfer.UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PeelTotal, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"股权转让: {transfer.Shares.ToString()}份", DbType.String);
            PeelRecordParams.Add("Source", 86, DbType.Int32);

            try
            {
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Int32 Rows = 0;
                        Rows += dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                        Rows += dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                        Rows += dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                        if (Rows != 3)
                        {
                            throw new Exception("扣除糖果失败[S]");
                        }
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug(transfer.GetJson(), ex);
                    }
                }
            }
            finally
            {
                if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); }
            }
            #endregion
            return result.SetStatus(ErrorCode.InvalidData, "股权转让失败");
        }

        /// <summary>
        /// 获取股权账务 信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<UserAccountEquity>> EquityInfo(long userId)
        {
            MyResult<UserAccountEquity> result = new MyResult<UserAccountEquity>();
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            UserAccountEquity accInfo = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountEquity>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            result.Data = accInfo;
            return result;
        }

        /// <summary>
        /// 股权记录
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <returns></returns>
        public async Task<MyResult<List<UserAccountEquityRecord>>> EquityRecords(QueryModel query)
        {
            MyResult<List<UserAccountEquityRecord>> result = new MyResult<List<UserAccountEquityRecord>>();
            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {query.UserId} LIMIT 1";
            UserAccountEquity accInfo = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountEquity>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
            result.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(1) AS `Count` FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId}");
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            if (query.PageIndex < 1) { query.PageIndex = 1; }
            IEnumerable<UserAccountEquityRecord> Records = await base.dbConnection.QueryAsync<UserAccountEquityRecord>($"SELECT * FROM {RecordTableName} WHERE `AccountId`={accInfo.AccountId} ORDER BY RecordId DESC LIMIT {(query.PageIndex - 1) * query.PageSize},{query.PageIndex * query.PageSize}");

            result.Data = new List<UserAccountEquityRecord>();
            foreach (var item in Records)
            {
                item.AccountId = 0;
                item.ModifyDesc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(","));
                result.Data.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 初始化股权账户
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        public async Task<MyResult<object>> InitWalletAccount(long userId)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String InsertSql = $"INSERT INTO `{AccountTableName}` (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`) VALUES ({userId}, '0', '0', '0', '0', NOW())";
            String SelectSql = $"SELECT COUNT(1) AS `Have` FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                int Have = await base.dbConnection.QueryFirstOrDefaultAsync<int>(SelectSql);
                if (Have != 0) { return result.SetStatus(ErrorCode.InvalidData, "账户已存在"); }
                int Row = await base.dbConnection.ExecuteAsync(InsertSql);
                if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, "账户初始化失败"); }
                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"初始化股权账户发生错误\r\n插入语句：{InsertSql}\r\n判断语句：{SelectSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        /// <summary>
        /// 股权冻结操作
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> FrozenEquity(long userId, decimal Amount)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={userId} AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                int Row = await base.dbConnection.ExecuteAsync(UpdateSql);
                if (Row != 1) { return result.SetStatus(ErrorCode.InvalidData, $"账户余额{(Amount > 0 ? "冻结" : "解冻")}操作失败"); }
                result.Data = true;
                return result;
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
        /// 股权账户余额变更
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        private async Task<MyResult<object>> ChangeWalletAmount(long userId, decimal Amount, EquityModifyType modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            UserAccountEquity UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}InitEquity_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} LIMIT 1";
                UserAccount = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountEquity>(SelectSql);
                if (UserAccount == null) { return result.SetStatus(ErrorCode.InvalidData, "股权不存在"); }
                if (Amount < 0)
                {
                    if (useFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "股权不足[F]"); }
                    }
                    else
                    {
                        if (UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "股权不足[B]"); }
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "可用股权不足[F]"); }
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
                base.dbConnection.Open();
                using (IDbTransaction Tran = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Int32 EditRow = base.dbConnection.Execute(EditSQl, null, Tran);
                        Int32 RecordId = base.dbConnection.Execute(RecordSql, null, Tran);
                        if (EditRow == RecordId && EditRow == 1)
                        {
                            Tran.Commit();
                            return new MyResult { Data = true };
                        }
                        Tran.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "股权变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        Yoyo.Core.SystemLog.Debug($"股权账户变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"股权变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }
    }
}
