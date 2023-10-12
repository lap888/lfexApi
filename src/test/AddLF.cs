using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.lfexentitys;
using domain.models.lfexDto;
using domain.repository;
using infrastructure.utils;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Xunit;

namespace test
{
    public class AddLF
    {
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";

        [Fact]
        public async Task AddLFM()
        {

            //Given
            using (IDbConnection dbConnection = new MySqlConnection("server=bj-cdb-061ud0fa.sql.tencentcdb.com;port=61617;user id=lfex_service_r;password=Guo123456-lfex_service_r;database=lfex_service_r;Charset=utf8mb4;"))
            {
                var rep = await ChangeWalletAmount(dbConnection, 810, "糖果", 696, LfexCoinnModifyType.Yyb_MoveCoin_Tome, false, "810_system", "696");
                System.Console.ReadLine();
            }

            //When

            //Then
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
        public async Task<MyResult<object>> ChangeWalletAmount(IDbConnection dbConnection, long userId, string coinType, decimal Amount, LfexCoinnModifyType modifyType, bool useFrozen, params string[] Desc)
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

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} AND `CoinType`='{coinType}' LIMIT 1";
                UserAccount = await dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
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
                if (dbConnection.State == ConnectionState.Closed) { dbConnection.Open(); }
                using (IDbTransaction Tran = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Int32 EditRow = dbConnection.Execute(EditSQl, null, Tran);
                        Int32 RecordId = dbConnection.Execute(RecordSql, null, Tran);
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
                    finally { if (dbConnection.State == ConnectionState.Open) { dbConnection.Close(); } }
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

    }
}
