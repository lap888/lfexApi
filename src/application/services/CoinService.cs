using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using application.services.bases;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.lfexentitys;
using domain.models;
using domain.models.lfexDto;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Yoyo.Core;

namespace application.services
{
    public class CoinService : BaseServiceLfex, ICoinService
    {
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";
        private readonly int CacheTime = 1 * 60 * 60;
        private readonly CSRedis.CSRedisClient RedisCache;

        private readonly HttpClient Client;
        private readonly HttpClient JPushSmsClient;
        private readonly Models.CoinConfig CoinConfig;
        public CoinService(IHttpClientFactory factory, IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redis, IOptionsMonitor<Models.CoinConfig> monitor) : base(connectionStringList)
        {
            this.RedisCache = redis;
            Client = factory.CreateClient("Coin");
            JPushSmsClient = factory.CreateClient("JPushSMS");
            this.CoinConfig = monitor.CurrentValue;
        }

        /// <summary>
        /// 获取账户币资产
        /// </summary>
        /// <param name="type">0 币币 1法币</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<CoinUserAccountWallet>> FindCoinAmount(int type, long userId)
        {
            MyResult<CoinUserAccountWallet> result = new MyResult<CoinUserAccountWallet>();
            if (string.IsNullOrWhiteSpace(type.ToString()))
            {
                return result.SetStatus(ErrorCode.InvalidData, "type 非法");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.NoAuth, "非法授权");
            }
            var sql = $"select uaw.*,(ct.`nowPrice`* uaw.`Balance`) UsPrice from (select * from `user_account_wallet` where UserId={userId} and TYPE={type}) uaw left join `coin_type` ct on ct.name=uaw.CoinType";
            var coinAmounts = await base.dbConnection.QueryAsync<UserAccountWalletModel>(sql);
            List<UserAccountWalletModel> userAccountWallets = new List<UserAccountWalletModel>();
            UserAccountWalletModel userAccountWalletModel = null;
            CoinUserAccountWallet coinUserAccountWallet = new CoinUserAccountWallet();
            decimal dPriceTotal = 0;
            decimal rPriceTotal = 0;
            coinAmounts.ToList().ForEach(action =>
            {
                userAccountWalletModel = new UserAccountWalletModel();
                userAccountWalletModel.Balance = action.Balance;
                userAccountWalletModel.AccountId = action.AccountId;
                userAccountWalletModel.Frozen = action.Frozen;
                userAccountWalletModel.CoinType = action.CoinType;
                userAccountWalletModel.UsPrice = type == 0 ? action.UsPrice : action.UsPrice * 7;
                userAccountWallets.Add(userAccountWalletModel);
                dPriceTotal += action.UsPrice;

            });
            rPriceTotal = dPriceTotal * 7;
            coinUserAccountWallet.DPriceTotal = dPriceTotal;
            coinUserAccountWallet.RPriceTotal = rPriceTotal;
            coinUserAccountWallet.Lists = userAccountWallets;

            result.Data = coinUserAccountWallet;
            return result;
        }

        /// <summary>
        /// 币排行
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<List<CoinTypeModel>>> FindCoinRank(QueryCoinRank model)
        {
            MyResult<List<CoinTypeModel>> result = new MyResult<List<CoinTypeModel>>();
            var flag = "";
            if (model.Type == 0)
            {
                //up
                flag = "order by upDown desc";

            }
            else if (model.Type == 1)
            {
                //down
                flag = "order by upDown";
            }
            else
            {
                //count
                flag = "order by count24 desc";
            }
            var flag2 = "";
            if (model.CoinType == 0)
            {
                //法币
                flag2 = "and type=0";

            }
            else
            {
                //币币
                flag2 = "and type=1";
            }
            var sqlStr = $"select * from `coin_type` where 1=1 {flag2} {flag}";
            var res = await base.dbConnection.QueryAsync<CoinType>(sqlStr);
            List<CoinTypeModel> coinTypeList = new List<CoinTypeModel>();
            CoinTypeModel coinTypeModel = null;
            int num = 1;
            res.ToList().ForEach(coinType =>
            {
                coinTypeModel = new CoinTypeModel();
                coinTypeModel.RankId = num;
                coinTypeModel.Name = coinType.Name;
                coinTypeModel.Count24 = coinType.Count24;
                coinTypeModel.CountTotal = coinType.CountTotal;
                coinTypeModel.NowPrice = coinType.NowPrice;
                coinTypeModel.LastPrice = coinType.LastPrice;
                coinTypeModel.UpDown = coinType.UpDown;
                coinTypeModel.Status = coinType.Status;
                coinTypeModel.Type = coinType.Type;
                coinTypeModel.Remark = coinType.Remark;
                coinTypeModel.CreateTime = coinType.CreateTime;
                coinTypeList.Add(coinTypeModel);
                num++;

            });
            result.Data = coinTypeList;
            return result;

        }

        /// <summary>
        /// 获取币种
        /// </summary>
        /// <returns></returns>
        public async Task<MyResult<List<CoinTypeModel>>> FindCoinType(int userId)
        {
            MyResult<List<CoinTypeModel>> result = new MyResult<List<CoinTypeModel>>();
            var sql = $"select ct.id,ct.name,ct.type,ct.status,ct.cstatus,ct.`fee`,ct.`minCanMove`,ct.`remark`,uaw.Balance balance from `coin_type` ct left join `user_account_wallet` uaw on ct.name = uaw.`CoinType` where userId={userId}";
            var query = await base.dbConnection.QueryAsync<CoinTypeModel>(sql);
            result.Data = query.ToList();
            return result;
        }

        /// <summary>
        /// 获取币种明细
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="ModifyType"></param>
        /// <returns></returns>
        public async Task<MyResult<List<UserAccountWalletRecord>>> CoinAccountRecord(long accountId, long userId, int PageIndex = 1, int PageSize = 20, LfexCoinnModifyType ModifyType = LfexCoinnModifyType.ALL)
        {
            MyResult<List<UserAccountWalletRecord>> result = new MyResult<List<UserAccountWalletRecord>>();
            if (PageIndex < 1) { PageIndex = 1; }

            String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} and `AccountId`={accountId} LIMIT 1";
            UserAccountWallet accInfo = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
            if (accInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "Coin不存在"); }
            #region 拼接SQL
            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("AccountId", accInfo.AccountId, DbType.Int64);

            StringBuilder QueryCountSql = new StringBuilder();
            QueryCountSql.Append("SELECT COUNT(1) AS `Count` FROM ");
            QueryCountSql.Append(RecordTableName);
            QueryCountSql.Append(" WHERE `AccountId` = @AccountId ");

            StringBuilder QueryDataSql = new StringBuilder();
            QueryDataSql.Append("SELECT * FROM ");
            QueryDataSql.Append(RecordTableName);
            QueryDataSql.Append(" WHERE `AccountId` = @AccountId ");
            if (ModifyType != LfexCoinnModifyType.ALL)
            {
                QueryParam.Add("ModifyType", (Int32)ModifyType, DbType.Int32);
                QueryCountSql.Append("AND `ModifyType` = @ModifyType ");
                QueryDataSql.Append("AND `ModifyType` = @ModifyType ");
            }
            QueryCountSql.Append(";");
            QueryParam.Add("PageIndex", (PageIndex - 1) * PageSize, DbType.Int32);
            QueryParam.Add("PageSize", PageSize, DbType.Int32);
            QueryDataSql.Append("ORDER BY RecordId DESC LIMIT @PageIndex,@PageSize;");
            #endregion

            result.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<int>(QueryCountSql.ToString(), QueryParam);
            result.PageCount = (result.RecordCount + PageSize - 1) / PageSize;
            IEnumerable<UserAccountWalletRecord> Records = await base.dbConnection.QueryAsync<UserAccountWalletRecord>(QueryDataSql.ToString(), QueryParam);

            result.Data = Records.ToList();
            return result;
        }

        /// <summary>
        /// 第三方查单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<CoinMoveRecord>> FindOrder(FindOrderModel model)
        {
            MyResult<CoinMoveRecord> result = new MyResult<CoinMoveRecord>();
            //验证签名
            var checkAdressSign = SecurityUtil.CheckAddressSign(model.Sign, Constants.YybKey, model.timeSpan);
            if (!checkAdressSign)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "签名非法");
            }

            if (string.IsNullOrWhiteSpace(model.OrderNum))
            {
                return result.SetStatus(ErrorCode.InvalidData, "orderNum 非法");
            }

            var sql = $"select * from `coin_move_record` where id='{model.OrderNum}'";
            var data = await base.dbConnection.QueryFirstOrDefaultAsync<CoinMoveRecord>(sql);
            if (data == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "订单不存在");
            }
            result.Data = data;
            return result;
        }


        /// <summary>
        /// 小鱼转币给第三方
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> MoveCoinToSomeone(MoveCoinToSomeoneModel model, int userId)
        {
            MyResult<object> result = new MyResult<object>();
            try
            {
                String CacheKey = $"MoveCoinToSomeone_Lock:{userId}";
                if (RedisCache.Exists(CacheKey))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "操作太快稍后再试...");
                }
                else { RedisCache.Set(CacheKey, userId, 30); }

                if (userId <= 0)
                {
                    return result.SetStatus(ErrorCode.ErrorSign, "签名错误");
                }
                if (model.CoinAmount < 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "非法数据");
                }
                if (model.CoinAmount.ToString().IsNumeric())
                {
                    return result.SetStatus(ErrorCode.InvalidData, "非法数据");
                }
                if (string.IsNullOrWhiteSpace(model.CoinName))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "币种类型异常");
                }
                //交易密码
                var userSql = $"select * from user where id={userId};";
                var userInfo = base.dbConnection.QueryFirstOrDefault<User>(userSql);
                var enPassword = SecurityUtil.MD5(model.Password);
                if (enPassword != userInfo.TradePwd)
                {
                    return result.SetStatus(ErrorCode.InvalidPassword, "交易密码错误");
                }
                //
                var coinBalance = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select Balance from `user_account_wallet` where userId={userId} and `CoinType`='{model.CoinName}'");
                var fee = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select fee from `coin_type` where name='{model.CoinName}'");
                if (coinBalance < model.CoinAmount)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "余额不足");
                }
                //验证等级卡限量
                var isMoveCoinToday = await base.dbConnection.QueryFirstOrDefaultAsync<int>($"select id from `coin_move_record` where type=0 and userId={userId} and status=1 and `coinType`='{model.CoinName}' and TO_DAYS(now())=TO_DAYS(`createTime`)");
                if (isMoveCoinToday > 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "今日已提币，请明日再提...");
                }

                var level = userInfo.Level.ToLower();
                var canMoveCoin = 0;
                if (level.Equals("lv0"))
                {
                    canMoveCoin = 100;
                }
                else if (level.Equals("lv1"))
                {
                    canMoveCoin = 500;
                }
                else if (level.Equals("lv2"))
                {
                    canMoveCoin = 2000;
                }
                else if (level.Equals("lv3"))
                {
                    canMoveCoin = 10000;
                }
                else if (level.Equals("lv4"))
                {
                    canMoveCoin = 20000;
                }
                else if (level.Equals("lv5"))
                {
                    canMoveCoin = 50000;
                }
                else
                {
                    canMoveCoin = 0;
                }
                if (model.CoinAmount > canMoveCoin && userInfo.Type != 1)
                {
                    return result.SetStatus(ErrorCode.InvalidData, $"您当前等级:{level}，最高提币额度:{canMoveCoin}");
                }
                //校验验证码
                var checkVcode = await CheckVcode(new domain.models.yoyoDto.ConfirmVcode { MsgId = model.MsgId, Mobile = model.Mobile, Vcode = model.VerifyCode });
                if (checkVcode.Code != 200)
                {
                    return result.SetStatus(ErrorCode.InvalidData, checkVcode.Message);
                }
                //提币记录写入
                long inserCoinMoveRecord = 0;
                try
                {
                    inserCoinMoveRecord = await base.dbConnection.ExecuteScalarAsync<long>($"insert into `coin_move_record`(userId,refId,address,amount,type,status,coinType) values ('{userId}','{userId}','{model.Adress}',{(model.CoinAmount - fee)},0,1,'{model.CoinName}');SELECT @@IDENTITY;");
                    if (inserCoinMoveRecord == 0)
                    {
                        return result.SetStatus(ErrorCode.SystemError, "写入区块失败");
                    }
                }
                catch (System.Exception ex)
                {
                    Yoyo.Core.SystemLog.Debug($"小鱼转给第三方平台 提币失败{model.ToJson()}", ex);
                    return result.SetStatus(ErrorCode.SystemError, "禁止重复提交");
                }
                //冻结
                var frozenInfo = await FrozenWalletAmount(userId, model.CoinAmount, model.CoinName);
                if (frozenInfo.Code != 200)
                {
                    SystemLog.Debug($"小鱼转出 冻结==model.CoinAmount={model.CoinAmount}fee={fee}userId={userId}");
                    return result.SetStatus(ErrorCode.InvalidData, "余额不足「error...」");
                }
                //通知第三方提币
                var _timestamp = DataConvertUtil.GetTimeStamp(DateTime.Now);
                var _sign = Sign(_timestamp, Constants.LittleFishKey);
                var requestJson = new { timestamp = _timestamp, sign = _sign, data = new { uuid = model.Adress, orderNum = inserCoinMoveRecord, amount = (model.CoinAmount - fee), coinName = model.CoinName } }.GetJson();
                StringContent content = new StringContent(requestJson);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.Client.PostAsync(CoinConfig.AccountToYoYoBa, content);
                String res = await response.Content.ReadAsStringAsync();
                var resData = res.GetModel<RepYybModel<YybMoveCoinTomeModel>>();
                if (resData == null)
                {
                    //调用第三方 提币失败 返回
                    await base.dbConnection.ExecuteAsync($"delete from `coin_move_record` where id={inserCoinMoveRecord}");
                    Yoyo.Core.SystemLog.Debug($"小鱼调用哟哟吧返回信息==>resData == null\r\n哟哟吧请求体:{requestJson}");
                    await FrozenWalletAmount(userId, -model.CoinAmount, model.CoinName);
                    return result.SetStatus(ErrorCode.SystemError, "第三方查单异常...");
                }
                if (resData.ErrCode != 0)
                {
                    //调用第三方 提币失败 返回
                    await base.dbConnection.ExecuteAsync($"delete from `coin_move_record` where id={inserCoinMoveRecord}");
                    Yoyo.Core.SystemLog.Debug($"小鱼调用哟哟吧返回信息{resData.ErrMsg}\r\n哟哟吧请求体:{requestJson}");
                    await FrozenWalletAmount(userId, -model.CoinAmount, model.CoinName);
                    return result.SetStatus(ErrorCode.SystemError, resData.ErrMsg);
                }
                var thridPardNum = resData.Data.OrderNo;

                var flag = LfexCoinnModifyType.MoveCoin_To_Yyb;
                if (model.CoinName.Equals("糖果"))
                {
                    flag = LfexCoinnModifyType.MoveCoin_To_Yyb;
                }
                else if (model.CoinName.Equals("YB"))
                {
                    flag = LfexCoinnModifyType.MoveCoin_To_Yyb_YB;
                }
                else if (model.CoinName.Equals("USDT(ERC20)"))
                {
                    flag = LfexCoinnModifyType.MoveCoin_To_Yyb_U;
                }
                else
                {
                    return result.SetStatus(ErrorCode.ClickFaster, "币种有误");
                }
                //调用第三方转币
                var rep = await ChangeWalletAmount(userId, model.CoinName, -model.CoinAmount, flag, true, $"{model.CoinAmount.ToString()}", $"{fee.ToString()}");
                //更新提币状态
                if (rep.Code == 200)
                {
                    await base.dbConnection.ExecuteAsync($"update `coin_move_record` set refId='{thridPardNum}',status=1 where id={inserCoinMoveRecord}");
                    return result;
                }
                else
                {
                    await base.dbConnection.ExecuteAsync($"update `coin_move_record` set status=2 where id={inserCoinMoveRecord}");
                    return result.SetStatus(ErrorCode.SystemError, "提币失败");
                }
            }
            catch (System.Exception ex)
            {
                SystemLog.Debug(ex);
                return result.SetStatus(ErrorCode.SystemError, "提币失败");
            }
        }
        /// <summary>
        /// 第三方平台转币个给小鱼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SomeoneMoveCoinTome(SomeMoveCoinToMeModel model)
        {
            MyResult result = new MyResult();
            var checkAdressSign = SecurityUtil.CheckAddressSign(model.Sign, Constants.YybKey, model.timeSpan);
            if (!checkAdressSign)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "签名非法");
            }
            //校验币名称
            if (string.IsNullOrWhiteSpace(model.CoinName))
            {
                return result.SetStatus(ErrorCode.ErrorSign, "提币币种不存在...");
            }
            var coinName = await base.dbConnection.QueryFirstOrDefaultAsync<int>($"select * from coin_type where name='{model.CoinName}'");
            if (coinName <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "提币币种不存在...");
            }
            var _timestamp = DataConvertUtil.GetTimeStamp(DateTime.Now);
            var _sign = Sign(_timestamp, Constants.LittleFishKey);
            StringContent content = new StringContent(new { timestamp = _timestamp, sign = _sign, data = model.RefId }.GetJson());
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.Client.PostAsync(CoinConfig.YybQueryTransfer, content);
            String res = await response.Content.ReadAsStringAsync();
            var redData = res.GetModel<RepYybModel<YybMoveCoinTomeModel>>();
            if (redData.ErrCode == 0)
            {
                //校验金额
                if (redData.Data.Amount != model.CoinAmount)
                {
                    return result.SetStatus(ErrorCode.NotFound, "金额校验失败");
                }
                //检查地址
                var userId = base.dbConnection.QueryFirstOrDefault<long>($"select id from user where `uuid`='{model.Adress}'");
                if (userId <= 0)
                {
                    return result.SetStatus(ErrorCode.NotFound, "提币地址有误");
                }
                //
                var isHadOrder = await base.dbConnection.QueryFirstOrDefaultAsync<int>($"select id from `coin_move_record` where refId='{model.RefId}'");
                if (isHadOrder > 0)
                {
                    return result.SetStatus(ErrorCode.ClickFaster, "订单地址已存在");
                }
                var inserCoinMoveRecord = await base.dbConnection.ExecuteAsync($"insert into `coin_move_record`(userId,refId,address,amount,type,status,coinType) values ('{model.OutUserUuid}','{model.RefId}','{model.Adress}',{model.CoinAmount},1,0,'{model.CoinName}')");
                if (inserCoinMoveRecord == 1)
                {
                    var flag = LfexCoinnModifyType.Yyb_MoveCoin_Tome;
                    if (model.CoinName.Equals("糖果"))
                    {
                        flag = LfexCoinnModifyType.Yyb_MoveCoin_Tome;
                    }
                    else if (model.CoinName.Equals("YB"))
                    {
                        flag = LfexCoinnModifyType.Yyb_MoveCoin_Tome_YB;
                    }
                    else if (model.CoinName.Equals("USDT(ERC20)"))
                    {
                        flag = LfexCoinnModifyType.Yyb_MoveCoin_Tome_U;
                    }
                    else
                    {
                        return result.SetStatus(ErrorCode.ClickFaster, "币种有误");
                    }
                    var rep = await ChangeWalletAmount(userId, model.CoinName, model.CoinAmount, flag, false, model.OutUserUuid, model.CoinAmount.ToString());
                    if (rep.Code == 200)
                    {
                        //更新订单记录
                        await base.dbConnection.ExecuteAsync($"update `coin_move_record` set status=1 where refId='{model.RefId}'");
                        return result;
                    }
                    else
                    {
                        SystemLog.Debug($"第三方提币给小鱼写入记录错误2：{model.GetJson()}");
                        return result.SetStatus(ErrorCode.SystemError, "写入币出错");
                    }
                }
                else
                {
                    SystemLog.Debug($"第三方提币给小鱼写入记录错误1：{model.GetJson()}");
                    return result.SetStatus(ErrorCode.SystemError, "写入币出错");
                }
            }
            else
            {
                return result.SetStatus(ErrorCode.InvalidData, redData.ErrMsg);
            }
        }
        private string Sign(long timeSpan, string key)
        {
            var signStr = $"{key}_{timeSpan}";
            return Security.MD5(signStr);
        }

        #region 钱包操作核心方法
        /// <summary>
        /// Coin钱包账户余额变更 common
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeWalletAmount(long userId, string coinType, decimal Amount, LfexCoinnModifyType modifyType, bool useFrozen, params string[] Desc)
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
                UserAccount = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
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
                TempRecordSql.Append($"'{DateTime.Now.ToString("yyyy-MM-d HH:mm:ss")}' AS`ModifyTime`");
                RecordSql = TempRecordSql.ToString();

                #region 修改账务
                if (base.dbConnection.State == ConnectionState.Closed) { base.dbConnection.Open(); }
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
                        return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
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
        public async Task<MyResult<object>> FrozenWalletAmount(long userId, decimal Amount, string coinType)
        {
            MyResult result = new MyResult { Data = false };
            CSRedisClientLock CacheLock = null;
            String UpdateSql = $"UPDATE `{AccountTableName}` SET `Frozen`=`Frozen`+{Amount} WHERE `UserId`={userId} AND `CoinType`='{coinType}' AND (`Balance`-`Frozen`)>={Amount} AND (`Frozen`+{Amount})>=0";
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
        /// 校验地址
        /// </summary>
        /// <param name="adress"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CheckAdress(CheckAdressModel model)
        {
            MyResult result = new MyResult();
            //验证签名
            var checkAdressSign = SecurityUtil.CheckAddressSign(model.Sign, Constants.YybKey, model.timeSpan);
            if (!checkAdressSign)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "签名非法");
            }
            var sql = $"select uuid,name from user where `uuid`='{model.Adress}'";

            var data = await base.dbConnection.QueryFirstOrDefaultAsync(sql);
            if (data == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "非法地址");
            }
            result.Data = data;
            return result;
        }
        #endregion

        public async Task<MyResult<object>> MoveCoinSendCode(string mobile, int userId)
        {
            MyResult result = new MyResult();
            var res = await CommonSendVcode(new domain.models.yoyoDto.SendVcode { Mobile = mobile, Type = "moveCoin" });
            if (res.Code != 200)
            {
                return result.SetStatus(ErrorCode.SystemError, res.Message);
            }
            result.Data = res.Data;
            return result;
        }

        #region 验证码模块
        /// <summary>
        /// 发送验证码公共方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<domain.models.yoyoDto.MsgDto>> CommonSendVcode(domain.models.yoyoDto.SendVcode model)
        {
            MyResult<domain.models.yoyoDto.MsgDto> result = new MyResult<domain.models.yoyoDto.MsgDto>();
            try
            {
                var MsgId = RedisCache.Get($"LfVCode:{ model.Mobile}");
                if (!String.IsNullOrWhiteSpace(MsgId))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "请10分钟后再试...");
                }

                StringContent content = new StringContent(new { mobile = model.Mobile, temp_id = "184448" }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.JPushSmsClient.PostAsync("https://api.sms.jpush.cn/v1/codes", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    String res = await response.Content.ReadAsStringAsync();
                    result.Data = res.GetModel<domain.models.yoyoDto.MsgDto>();
                    RedisCache.Set($"LfVCode:{ model.Mobile}", result.Data.Msg_Id, 600);
                }
                else
                {
                    return new MyResult<domain.models.yoyoDto.MsgDto> { Data = new domain.models.yoyoDto.MsgDto { Is_Valid = false, Msg_Id = null, Error = new domain.models.yoyoDto.ErrorDto { Code = "-1", Message = result.Data.Error.Message } } };
                }
            }
            catch (System.Exception)
            {
                var res = "{\"error\":{\"code\":50013,\"message\":\"invalid temp_id\"}}";
                var resModel = res.GetModel<domain.models.yoyoDto.MsgDto>();
                result.Data = resModel;
                return result;
            }
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", model.Mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await base.dbConnection.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<domain.models.yoyoDto.MsgDto>> CheckVcode(domain.models.yoyoDto.ConfirmVcode model)
        {
            MyResult<domain.models.yoyoDto.MsgDto> result = new MyResult<domain.models.yoyoDto.MsgDto>();
            try
            {
                var MsgId = RedisCache.Get($"LfVCode:{ model.Mobile}");

                if (String.IsNullOrWhiteSpace(MsgId) || MsgId != model.MsgId)
                {
                    return result.SetStatus(ErrorCode.HasValued, "验证码非法");
                }

                StringContent content = new StringContent(new { code = model.Vcode }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.JPushSmsClient.PostAsync($"https://api.sms.jpush.cn/v1/codes/{model.MsgId}/valid", content);
                String res = await response.Content.ReadAsStringAsync();

                var resModel = res.GetModel<domain.models.yoyoDto.MsgDto>();

                if (resModel.Is_Valid)
                {
                    RedisCache.Del($"LfVCode:{ model.Mobile}");
                }
                else
                {
                    return result.SetStatus(ErrorCode.HasValued, resModel.Error.Message);
                }

                result.Data = resModel;
            }
            catch (System.Exception)
            {
                return result.SetStatus(ErrorCode.HasValued, "验证码非法");
            }
            return result;
        }
        #endregion

        #region 矿机
        public async Task<MyResult<List<MinningDto>>> MinningList(int userId)
        {
            MyResult<List<MinningDto>> result = new MyResult<List<MinningDto>>();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "sign error");
            }
            IEnumerable<Minnings> task;
            task = await base.dbConnection.QueryAsync<Minnings>($"select * from minnings where userId = {userId} and endTime > now() order by id");

            List<MinningDto> minningDtoList = new List<MinningDto>();
            //名称过滤重组
            task.ToList().ForEach(t =>
            {
                MinningDto minningDto = new MinningDto();
                var name = t.Source == 2 ? "🎖" : "";
                minningDto.MinningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).MinningName + name;
                minningDto.Pow = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).Pow;
                minningDto.Colors = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).Colors;

                minningDto.Id = t.Id;
                minningDto.UserId = t.UserId;
                minningDto.MinningId = t.MinningId;
                minningDto.BeginTime = t.BeginTime;
                minningDto.EndTime = t.EndTime;
                minningDto.Status = t.Status;
                minningDto.WorkingTime = t.WorkingTime?.ToString("yyyy/MM/dd HH:mm:ss") ?? "0";
                minningDto.WorkingEndTime = t.WorkingEndTime?.ToString("yyyy/MM/dd HH:mm:ss") ?? "0";
                minningDto.Source = t.Source;
                minningDto.MinningStatus = t.MinningStatus;
                minningDtoList.Add(minningDto);
            });
            result.Data = minningDtoList;
            return result;
        }

        /// <summary>
        /// 开始任务
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="mId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> DoTask(int userId, int mId)
        {
            MyResult result = new MyResult();
            if (userId < 0) { return result.SetStatus(ErrorCode.InvalidToken, "sign error"); }

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {

                //=====================================使用Redis分布式锁=====================================//
                String CacheKey = $"DoTask_Lock:{userId}_{mId}";
                if (RedisCache.Exists(CacheKey))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您操作太快了");
                }
                else { RedisCache.Set(CacheKey, userId, 30); }

                CacheLock = RedisCache.Lock($"DoTask:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//
                StringBuilder QueryTaskIds = new StringBuilder();
                QueryTaskIds.Append("SELECT `minningId`,`status` FROM `minnings` WHERE `userId` = @UserId AND `status` = 1 ");
                QueryTaskIds.Append("AND `beginTime` < Now() AND `endTime` > Now() ");
                QueryTaskIds.Append("AND `id` = @MinningId");
                DynamicParameters QueryTaskIdsParam = new DynamicParameters();
                QueryTaskIdsParam.Add("UserId", userId, DbType.Int32);
                QueryTaskIdsParam.Add("MinningId", mId, DbType.Int32);
                var minnings = await base.dbConnection.QueryFirstOrDefaultAsync<Minnings>(QueryTaskIds.ToString(), QueryTaskIdsParam);
                if (minnings == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "该矿机未生效请明日挖矿...");
                }
                if (minnings.Status == 0)
                {
                    return result.SetStatus(ErrorCode.TaskHadDo, "当前矿机已经损坏，请去维修");
                }
                if (minnings.MinningStatus == 2)
                {
                    return result.SetStatus(ErrorCode.TaskHadDo, "此矿机挖矿已经完成...");
                }

                User UserInfo = base.dbConnection.QueryFirstOrDefault<User>($"SELECT `status`, auditState, `level`, `name`, candyNum, inviterMobile, ctime FROM `user` WHERE id={userId};");
                if (UserInfo.Status != 0) { return result.SetStatus(ErrorCode.SystemError, "账号状态异常"); }
                if (UserInfo.AuditState != 2) { return result.SetStatus(ErrorCode.SystemError, "未实名不能挖矿!"); }

                YoyoTaskRecord TaskRecord = dbConnection.QueryFirstOrDefault<YoyoTaskRecord>($"SELECT * FROM yoyo_task_record WHERE UserId = @UserId AND Source = 0 AND MId={mId} AND `CreateDate` = DATE(NOW());",
                    new { UserId = userId });
                if (TaskRecord == null && (DateTime.Now.Hour < 6 || DateTime.Now.Hour > 20))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "挖矿时间为6-20点....");
                }
                if (TaskRecord == null)
                {
                    DateTime EndTime = DateTime.Now.AddHours(3);
                    dbConnection.Execute("INSERT INTO `yoyo_task_record`(`UserId`,`MId`, `Schedule`, `Source`, `CreateDate`, `StartTime`, `EndTime`, `UpdateDate`) VALUES (@UserId,@MId, 0, 0, DATE(NOW()), NOW(), @EndTime , NOW());",
                        new { UserId = userId, EndTime = EndTime, MId = mId });

                    result.Data = new { StartTime = DateTime.Now, EndTime = EndTime, QuickenMinutes = 0 };
                    await dbConnection.ExecuteAsync($"update `minnings` set `minningStatus`=1,workingTime=NOW(),workingEndTime='{EndTime.ToString("yyyy-MM-d HH:mm:ss")}' where id={mId}");

                    return result;
                }
                if (TaskRecord.EndTime > DateTime.Now)
                {
                    Double TaskTime = ((DateTime)TaskRecord.EndTime - DateTime.Now).TotalSeconds;

                    if (TaskTime > 0)
                    {
                        return result.SetStatus(ErrorCode.SystemError, "正在努力挖矿中...");
                    }
                }


                Int64 ReferrerId = base.dbConnection.QueryFirstOrDefault<Int64>("SELECT id FROM `user` WHERE mobile = @Mobile;", new { Mobile = UserInfo.InviterMobile });

                decimal DayCandyOut = 0;    //我的任务产量
                decimal fDayPowOut = 0;
                DayCandyOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minnings.MinningId).Pow;
                fDayPowOut = DayCandyOut * 0.05M;
                var totalCandyToday = DayCandyOut;
                //更新LF钱包
                var minningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minnings.MinningId).MinningName;

                var res1 = await ChangeWalletAmount(userId, "LF", DayCandyOut, LfexCoinnModifyType.Lf_Coin_Day_In, false, $"{minningName}", $"{DayCandyOut.ToString()}");
                if (res1.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, "挖矿奖励发放失败");
                }
                if (ReferrerId != 0)
                {
                    await ChangeWalletAmount(ReferrerId, "LF", fDayPowOut, LfexCoinnModifyType.Lf_Xia_Ji_Give, false, $"[{UserInfo.Name}]", $"{fDayPowOut.ToString()}");
                }
                var c1Num = Math.Round(DayCandyOut, 4);
                var c1 = RedisCache.Publish("Lfex_Member_LFChange_Signle", JsonConvert.SerializeObject(new { bId = userId, bBalance = c1Num }));
                //已完成当日挖矿
                await dbConnection.ExecuteAsync($"update `minnings` set `minningStatus`=2 where id={mId}");
                result.Data = new { StartTime = DateTime.Now.ToString("yyyy-MM-d HH:mm:ss"), EndTime = DateTime.Now.ToString("yyyy-MM-d HH:mm:ss") };
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("挖矿 => 异常：", ex);
                return result.SetStatus(ErrorCode.SystemError, "挖矿失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }
        public async Task<MyResult<object>> RepairMinning(int userId, int mId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "sign error");
            }
            //查矿机序列号
            var isHadMinning = await base.dbConnection.QuerySingleAsync<Minnings>($"select * from minnings where id={mId}");
            if (isHadMinning == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "矿机序列号查询失败");
            }
            var minningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == isHadMinning.MinningId).MinningName;
            var res1 = await ChangeWalletAmount(userId, "LF", -1M, LfexCoinnModifyType.Lf_Repair_Minning, false, minningName, $"{1.ToString()}");
            var sqlStr = $"update `minnings` set `status`=1 where id={mId}";
            if (res1.Code != 200)
            {
                return result.SetStatus(ErrorCode.SystemError, res1.Message);
            }
            await base.dbConnection.ExecuteAsync(sqlStr);
            return result;
        }
        #endregion
        /// <summary>
        /// 贡献值流水
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> GlodsRecord(BaseModel model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign error");
            }
            //查询贡献值记录
            var candyPLists = base.dbConnection.Query<UserCandyp>($"select * from `user_candyp` where `userId`={userId} and `source` = 1 order by id desc").AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = candyPLists;
            return result;
        }

        public async Task<MyResult<object>> LookUpIncomeSetting()
        {
            MyResult result = new MyResult();
            String CacheKey = $"LookUpIncomeSetting";
            if (RedisCache.Exists(CacheKey))
            {
                return RedisCache.Get<MyResult<object>>(CacheKey);
            }
            else
            {
                var candyPLists = await base.dbConnection.QueryAsync($"select * from `look_up_setting`");
                result.Data = candyPLists;
                var cacheString = candyPLists.ToJson(false, true, true);
                this.RedisCache.Set(CacheKey, result, CacheTime, RedisExistence.Nx);
            }
            return result;
        }
        public async Task<MyResult<object>> ConfirmLookUp(int userId, int type, decimal amount)
        {
            MyResult<object> result = new MyResult<object>();
            try
            {
                String CacheKey = $"ConfirmLookUp:{userId}_{type}";
                if (RedisCache.Exists(CacheKey))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您操作太快了");
                }
                else { RedisCache.Set(CacheKey, userId, 30); }
                if (userId <= 0)
                {
                    return result.SetStatus(ErrorCode.ErrorSign, "Sign Error...");
                }
                if (amount <= 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "amount 不能为空...");
                }
                var setting = await base.dbConnection.QueryFirstOrDefaultAsync<LookUpSettingModel>($"select * from `look_up_setting` where type={type}");
                if (setting == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "锁仓类型有误");
                }
                //记录
                var StartTime = DateTime.Now.ToString("yyyy-MM-d");
                var EndTime = DateTime.Now.ToString("yyyy-MM-d");
                if (type == 1)
                {
                    EndTime = DateTime.Now.AddDays(30).ToString("yyyy-MM-d");
                }
                else if (type == 2)
                {
                    EndTime = DateTime.Now.AddDays(60).ToString("yyyy-MM-d");
                }
                else if (type == 3)
                {
                    EndTime = DateTime.Now.AddDays(90).ToString("yyyy-MM-d");
                }
                else
                {
                    EndTime = DateTime.Now.ToString("yyyy-MM-d");
                }
                var rep = await ChangeWalletAmount(userId, "LF", -amount, LfexCoinnModifyType.Look_Up_Minners, false, amount.ToString());
                if (rep.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, rep.Message);
                }
                var res = await base.dbConnection.ExecuteAsync($"insert into `look_up_income`(`id`,`userId`,`amount`,type,`startTime`,`endTime`) values ('{Gen.NewGuid()}',{userId},{amount},{type},'{StartTime}','{EndTime}')");
                if (res <= 0)
                {
                    return result.SetStatus(ErrorCode.SystemError, "写入锁仓订单失败...");
                }
            }
            catch (System.Exception ex)
            {
                SystemLog.Debug($"ConfirmLookUp==>{ex}");
                return result.SetStatus(ErrorCode.SystemError, "锁仓订单失败...");
            }

            return result;
        }

        //矿池订单
        public async Task<MyResult<object>> MinnersOrder(LooKUpMinnerModel model, int userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Sign Error...");
            }
            if (model.Status < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "Status 不能为空...");
            }
            var candyPLists = (await base.dbConnection.QueryAsync($"select * from `look_up_income` where status={model.Status} and userId={userId}")).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = candyPLists;
            return result;
        }
        //赎回
        public async Task<MyResult<object>> SopOrder(int userId, string orderNum)
        {
            MyResult result = new MyResult();
            try
            {
                String CacheKey = $"Sop_Order:{userId}_{orderNum}";
                if (RedisCache.Exists(CacheKey))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您操作太快了");
                }
                else { RedisCache.Set(CacheKey, userId, 30); }
                if (userId <= 0)
                {
                    return result.SetStatus(ErrorCode.ErrorSign, "Sign Error...");
                }
                if (string.IsNullOrWhiteSpace(orderNum))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "orderNum 不能为空...");
                }
                var orderInfo = await base.dbConnection.QueryFirstOrDefaultAsync<LookUpIncomeModel>($"select * from `look_up_income` where status=0 and userId={userId} and id='{orderNum}'");
                if (orderInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单失效...");
                }
                if (DateTime.Now < orderInfo.EndTime)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "未满锁仓日期，禁止赎回...");
                }
                if (orderInfo.Status == 1)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单失效...");
                }
                var res = await base.dbConnection.ExecuteAsync($"update `look_up_income` set `status`=1,`sopTime`=now() where id='{orderNum}'");
                if (res <= 0)
                {
                    return result.SetStatus(ErrorCode.SystemError, "更新订单失败...");
                }
                var amount = Math.Round(orderInfo.Amount, 4);
                var rep = await ChangeWalletAmount(userId, "LF", amount, LfexCoinnModifyType.Look_Up_Sop, false, amount.ToString());
                if (rep.Code != 200)
                {
                    return result.SetStatus(ErrorCode.SystemError, rep.Message);
                }
            }
            catch (System.Exception ex)
            {
                SystemLog.Debug($"赎回==>{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// KLine 0 分时 1 15分钟 2 1小时 3 4小时 4 1天 5 1个月
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> KLine(QueryKLine model)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(model.CoinType))
            {
                return result.SetStatus(ErrorCode.SystemError, "币种类型不能为空...");
            }
            var ishaveCoinTypeSql = $"select * from `coin_type` where `name`='{model.CoinType}'";
            var ishaveCoinType = await base.dbConnection.QueryFirstOrDefaultAsync(ishaveCoinTypeSql);

            if (ishaveCoinType == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "币种有误...");
            }

            var dateSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,DATE(`ctime`) AS s,UNIX_TIMESTAMP(`ctime`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from `coin_trade` where `coinType`='{model.CoinType}' group by s order by s limit 200) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id";
            var weekSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,WEEK(`ctime`) AS s,UNIX_TIMESTAMP(`ctime`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from `coin_trade` where `coinType`='{model.CoinType}' group by s order by s limit 200) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id";
            var monthSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,MONTH(`ctime`) AS s,UNIX_TIMESTAMP(`ctime`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from `coin_trade` where `coinType`='{model.CoinType}' group by s order by s limit 200) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id";
            var hourSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,DATE_FORMAT(`ctime`, '%Y-%m-%d %H:00:00') AS s,UNIX_TIMESTAMP(`ctime`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from `coin_trade` where `coinType`='{model.CoinType}' group by s order by s desc limit 500) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id order by s asc";
            var i = 15;
            var timeSql = "";
            switch (model.Type)
            {
                case 0:
                    i = 1;
                    timeSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,s,UNIX_TIMESTAMP(`s`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from (select *,DATE_FORMAT(concat(date( `ctime` ), ' ', HOUR ( `ctime` ), ':', floor( MINUTE ( `ctime` ) / {i} ) * {i} ),'%Y-%m-%d %H:%i') as s from `coin_trade`) a where `coinType`='{model.CoinType}' group by s order by s desc limit 500) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id order by s asc";
                    result.Data = await base.dbConnection.QueryAsync(timeSql);
                    break;

                case 1:
                    i = 15;
                    timeSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,s,UNIX_TIMESTAMP(`s`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from (select *,DATE_FORMAT(concat(date( `ctime` ), ' ', HOUR ( `ctime` ), ':', floor( MINUTE ( `ctime` ) / {i} ) * {i} ),'%Y-%m-%d %H:%i') as s from `coin_trade`) a where `coinType`='{model.CoinType}' group by s order by s desc limit 500) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id order by s asc";
                    result.Data = await base.dbConnection.QueryAsync(timeSql);
                    break;
                case 2:
                    i = 30;
                    timeSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,s,UNIX_TIMESTAMP(`s`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from (select *,DATE_FORMAT(concat(date( `ctime` ), ' ', HOUR ( `ctime` ), ':', floor( MINUTE ( `ctime` ) / {i} ) * {i} ),'%Y-%m-%d %H:%i') as s from `coin_trade`) a where `coinType`='{model.CoinType}' group by s order by s desc limit 500) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id order by s asc";
                    result.Data = await base.dbConnection.QueryAsync(timeSql);
                    break;
                case 3:
                    result.Data = await base.dbConnection.QueryAsync(hourSql);
                    break;
                case 4:
                    result.Data = await base.dbConnection.QueryAsync(dateSql);
                    break;
                case 5:
                    result.Data = await base.dbConnection.QueryAsync(monthSql);
                    break;
                case 6:
                    result.Data = await base.dbConnection.QueryAsync(monthSql);
                    break;
                default:
                    i = 15;
                    timeSql = $"select a.id,a.`amount`,a.vol,a.high,a.low,a.count,b.price open,c.price close from (select Max(id) maxId,Min(id) minId,s,UNIX_TIMESTAMP(`s`) id,sum(amount*price*7) AS 'amount',sum(amount) AS 'vol',MAX(price) high,MIN(price) low,1 count from (select *,DATE_FORMAT(concat(date( `ctime` ), ' ', HOUR ( `ctime` ), ':', floor( MINUTE ( `ctime` ) / {i} ) * {i} ),'%Y-%m-%d %H:%i') as s from `coin_trade`) a where `coinType`='{model.CoinType}' group by s order by s desc limit 500) a left join `coin_trade` b on a.maxId=b.id left join `coin_trade` c on a.minId=c.id order by s asc";
                    result.Data = await base.dbConnection.QueryAsync(timeSql);
                    break;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> KLinePanel(string coinType)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(coinType))
            {
                return result.SetStatus(ErrorCode.SystemError, "币种类型不能为空...");
            }
            var ishaveCoinTypeSql = $"select * from `coin_type` where `name`='{coinType}'";
            var ishaveCoinType = await base.dbConnection.QueryFirstOrDefaultAsync(ishaveCoinTypeSql);
            if (ishaveCoinType == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "币种有误...");
            }
            var sql = $"select *,IFNULL((nowPrice-avgPrice),0)*100 rate from (select IFNULL(sum(amount),0) 24amount,IFNULL(Max(price),0) todayMax,IFNULL(Min(price),0) todayMin,IFNULL(AVG(`price`),0) avgPrice,IFNULL(AVG(`price`),0)*7 rmbAvgPrice from `coin_trade` where TO_DAYS(now())-TO_DAYS(dealTime)=1 and `coinType`='{coinType}') a join (select IFNULL(price,0) nowPrice,IFNULL(SUM(price),0) sPrice from `coin_trade` where `coinType`='{coinType}' order by id desc limit 1) b";
            result.Data = await base.dbConnection.QueryFirstOrDefaultAsync(sql);
            return result;
        }

        public async Task<MyResult<object>> NewCoinData(string coinType)
        {
            MyResult result = new MyResult();
            var ishaveCoinTypeSql = $"select * from `coin_type` where `name`='{coinType}'";
            var ishaveCoinType = await base.dbConnection.QueryFirstOrDefaultAsync(ishaveCoinTypeSql);
            if (ishaveCoinType == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "币种有误...");
            }
            var buyWSql = $"select id,trendSide,amount,price,0.1 rate  from `coin_trade` where `trendSide`='BUY' and `coinType`='{coinType}' and status=1 order by price desc limit 5";
            var sellWSql = $"select id,trendSide,amount,price,0.1 rate from `coin_trade` where `trendSide`='SELL' and `coinType`='{coinType}' and status=1 order by price desc limit 5";
            var buyW = await base.dbConnection.QueryAsync(buyWSql);
            var sellW = await base.dbConnection.QueryAsync(sellWSql);

            var buySumAmountSql = $"select IFNULL(sum(amount),0) from (select amount  from `coin_trade` where `trendSide`='BUY' and `coinType`='{coinType}' and status=1 order by price desc limit 5) a";
            var sellSumAmountSql = $"select IFNULL(sum(amount),0) from (select amount  from `coin_trade` where `trendSide`='SELL' and `coinType`='{coinType}' and status=1 order by price desc limit 5) a";
            var buySumAmount = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>(buySumAmountSql);
            var sellSumAmount = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>(sellSumAmountSql);
            buyW.ToList().ForEach(action =>
            {
                action.rate = action.amount / buySumAmount;

            });
            sellW.ToList().ForEach(action =>
            {
                action.rate = action.amount / sellSumAmount;
            });
            result.Data = new { buyW = buyW, sellW = sellW };
            return result;
        }
        /// <summary>
        /// type =0 委托订单 type =1 最新成交  type=2 币种简介
        /// </summary>
        /// <param name="type"></param>
        /// <param name="coinType"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> CoinData(int type, string coinType)
        {
            MyResult result = new MyResult();
            var ishaveCoinTypeSql = $"select * from `coin_type` where `name`='{coinType}'";
            var ishaveCoinType = await base.dbConnection.QueryFirstOrDefaultAsync(ishaveCoinTypeSql);
            if (ishaveCoinType == null)
            {
                return result.SetStatus(ErrorCode.SystemError, "币种有误...");
            }
            //委托订单
            if (type == 0)
            {
                var buyWSql = $"select id,trendSide,amount,price,0.1 rate  from `coin_trade` where `trendSide`='BUY' and `coinType`='{coinType}' and status=1 order by price desc limit 20";
                var sellWSql = $"select id,trendSide,amount,price,0.1 rate from `coin_trade` where `trendSide`='SELL' and `coinType`='{coinType}' and status=1 order by price desc limit 20";
                var buyW = await base.dbConnection.QueryAsync(buyWSql);
                var sellW = await base.dbConnection.QueryAsync(sellWSql);

                var buySumAmountSql = $"select IFNULL(sum(amount),0) from (select amount  from `coin_trade` where `trendSide`='BUY' and `coinType`='{coinType}' and status=1 order by price desc limit 20) a";
                var sellSumAmountSql = $"select IFNULL(sum(amount),0) from (select amount  from `coin_trade` where `trendSide`='SELL' and `coinType`='{coinType}' and status=1 order by price desc limit 20) a";
                var buySumAmount = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>(buySumAmountSql);
                var sellSumAmount = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>(sellSumAmountSql);
                buyW.ToList().ForEach(action =>
                {
                    action.rate = action.amount / buySumAmount;

                });
                sellW.ToList().ForEach(action =>
                {
                    action.rate = action.amount / sellSumAmount;
                });
                result.Data = new { buyW = buyW, sellW = sellW };
            }
            else if (type == 1)
            {
                var finishSql = $"select id,trendSide,amount,price,dealTime  from `coin_trade` where `coinType`='{coinType}' and status=4 order by dealTime desc limit 20";
                var data = await base.dbConnection.QueryAsync<NewTModel>(finishSql);
                data.ToList().ForEach(action =>
                {
                    action.DealTime = DateTime.Parse(action.DealTime.ToString("yyyy-MM-dd HH:mm"));
                });
                result.Data = data;
            }
            else
            {
                var remarkSql = $"select remark from `coin_type` where `name`='{coinType}'";
                result.Data = await base.dbConnection.QueryFirstOrDefaultAsync(remarkSql);
            }
            return result;
        }
    }
}
public class NewTModel
{
    public int Id { get; set; }
    public string TrendSide { get; set; }
    public decimal Amount { get; set; }
    public decimal Price { get; set; }
    public DateTime DealTime { get; set; }

}