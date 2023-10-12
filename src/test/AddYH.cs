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
    public class AddYH
    {
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";


        [Fact]
        public async Task YH()
        {
            CSRedisClient RedisCache = new CSRedisClient("129.28.186.13:6379,password=yoyoba,defaultDatabase=7,prefix=G_");
            //Given
            using (IDbConnection dbConnection = new MySqlConnection("server=bj-cdb-061ud0fa.sql.tencentcdb.com;port=61617;user id=lfex_service_r;password=Guo123456-lfex_service_r;database=lfex_service_r;Charset=utf8mb4;"))
            {
                int[] authIds = new int[]
                {
                    6749,
                    6750,
                    6751,
                    6752,
                    6753,
                    6754,
                    6755,
                    6756,
                    6757,
                    6758,
                    6759,
                    6760,
                    6761,
                    6762,
                    6763,
                    6764,
                    6765,
                    6766,
                    6767,
                    6768,
                    6769,
                    6770,
                    6771,
                    6772,
                    6773,
                    6774,
                    6775,
                    6777,
                    6778,
                    6779,
                    6780,
                    6781,
                    6782,
                    6783,
                    6784,
                    6785,
                    6786,
                    6787,
                    6788,
                    6789,
                    6791,
                    6792,
                    6793,
                    6794,
                    6795,
                    6796,
                    6797,
                    6798,
                    6799,
                    6800
                };
                dbConnection.Open();
                Random random = new Random();
                int[] timeNum = new int[] { 20000, 60000, 15000, 60000, 19000, 50000, 90000, 100000, 63000, 120000, 140000 };
                // int[] timeNum = new int[] { 20000 };
                for (int i = 0; i < authIds.Length; i++)
                {
                    int time = timeNum[random.Next(0, timeNum.Length - 1)];
                    var res = await DoTask(dbConnection, authIds[i], 0, RedisCache);
                    System.Console.WriteLine($"ID:{i},用户:{authIds[i]},{res.Message}");
                    System.Threading.Thread.Sleep(time);
                }
                dbConnection.Close();
                System.Console.WriteLine("挖矿已完成...");
                System.Console.ReadLine();
            }

            //When

            //Then
        }
        public async Task<MyResult<object>> DoTask(IDbConnection dbConnection, int userId, int mId, CSRedisClient RedisCache)
        {
            MyResult result = new MyResult();
            StringBuilder QueryTaskIds = new StringBuilder();
            QueryTaskIds.Append("SELECT `minningId`,`status` FROM `minnings` WHERE `userId` = @UserId AND `status` = 1 ");
            QueryTaskIds.Append("AND `beginTime` < Now() AND `endTime` > Now() ");
            QueryTaskIds.Append("AND `minningId` = @MinningId");
            DynamicParameters QueryTaskIdsParam = new DynamicParameters();
            QueryTaskIdsParam.Add("UserId", userId, DbType.Int32);
            QueryTaskIdsParam.Add("MinningId", mId, DbType.Int32);
            var minnings = await dbConnection.QueryFirstOrDefaultAsync<Minnings>(QueryTaskIds.ToString(), QueryTaskIdsParam);
            if (minnings == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "该矿机未生效请明日挖矿...");
            }
            if (minnings.Status == 0)
            {
                return result.SetStatus(ErrorCode.TaskHadDo, "当前矿机已经损坏，请去维修");
            }
            User UserInfo = dbConnection.QueryFirstOrDefault<User>($"SELECT `status`, auditState, `level`, `name`, candyNum, inviterMobile, ctime FROM `user` WHERE id={userId};");
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


            Int64 ReferrerId = dbConnection.QueryFirstOrDefault<Int64>("SELECT id FROM `user` WHERE mobile = @Mobile;", new { Mobile = UserInfo.InviterMobile });

            decimal DayCandyOut = 0;    //我的任务产量
            decimal fDayPowOut = 0;
            DayCandyOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minnings.MinningId).Pow;
            fDayPowOut = DayCandyOut * 0.05M;
            var totalCandyToday = DayCandyOut;
            //更新LF钱包
            var minningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minnings.MinningId).MinningName;

            var res1 = await ChangeWalletAmount(dbConnection, userId, "LF", DayCandyOut, LfexCoinnModifyType.Lf_Coin_Day_In, false, $"{minningName}", $"{DayCandyOut.ToString()}");
            if (res1.Code != 200)
            {
                return result.SetStatus(ErrorCode.SystemError, "挖矿奖励发放失败");
            }
            if (ReferrerId != 0)
            {
                await ChangeWalletAmount(dbConnection, ReferrerId, "LF", fDayPowOut, LfexCoinnModifyType.Lf_Xia_Ji_Give, false, $"[{UserInfo.Name}]", $"{fDayPowOut.ToString()}");
            }
            var c1Num = Math.Round(DayCandyOut, 4);
            var c1 = RedisCache.Publish("Lfex_Member_LFChange_Signle", JsonConvert.SerializeObject(new { bId = userId, bBalance = c1Num }));
            //已完成当日挖矿
            await dbConnection.ExecuteAsync($"update `minnings` set `minningStatus`=2 where id={mId}");
            result.Data = new { StartTime = DateTime.Now.ToString("yyyy-MM-d HH:mm:ss"), EndTime = DateTime.Now.ToString("yyyy-MM-d HH:mm:ss") };

            return result;
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
