using application.services.bases;
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace application.services
{
    public class SystemService : BaseServiceLfex, ISystemService
    {
        private readonly CSRedisClient RedisCache;
        private readonly IEquityService EquitySub;
        private readonly Models.AppSetting AppSetting;
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";
        public SystemService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient, IEquityService equityService, IOptionsMonitor<Models.AppSetting> monitor) : base(connectionStringList)
        {
            RedisCache = redisClient;
            EquitySub = equityService;
            AppSetting = monitor.CurrentValue;
        }

        public MyResult<object> AppDownloadUrl()
        {
            MyResult result = new MyResult();
            var androidClientVersion = base.dbConnection.QueryFirstOrDefault<SysClientVersions>($"select * from `sys_client_versions` where `deviceSystem`='android' order by id desc limit 1");
            var iosClientVersion = base.dbConnection.QueryFirstOrDefault<SysClientVersions>($"select * from `sys_client_versions` where `deviceSystem`='ios' order by id desc limit 1");
            result.Data = new { ios = iosClientVersion.DownloadUrl, android = androidClientVersion.DownloadUrl };
            return result;
        }

        public SysBanner Banner(int id)
        {
            try
            {
                String CacheKey = $"BannerInfo:{id}";
                if (RedisCache.Exists(CacheKey)) { return RedisCache.Get<SysBanner>(CacheKey); }
                var bannner = base.dbConnection.QueryFirst<SysBanner>($"select * from sys_banner where id={id}");
                if (bannner == null) { return null; }
                RedisCache.Set(CacheKey, bannner, 60 * 60);
                return bannner;
            }
            catch
            {
                return null;
            }
        }

        public async Task<MyResult<object>> CopyWriting(string type)
        {
            MyResult<object> result = new MyResult<object> { Data = new List<CopyWriting>() };
            if (String.IsNullOrWhiteSpace(type)) { return result; }
            string CacheKey = $"System:Copys_{type}";
            if (RedisCache.Exists(CacheKey))
            {
                if (type.Equals("real_name_rule", StringComparison.OrdinalIgnoreCase))
                {
                    result.Data = RedisCache.Get<List<CopyWriting>>(CacheKey).FirstOrDefault().Text;
                    return result;
                }
                result.Data = RedisCache.Get<List<CopyWriting>>(CacheKey);
                return result;
            }
            List<CopyWriting> dbdata = (await base.dbConnection.QueryAsync<CopyWriting>("SELECT `key`,`title`,`text` FROM `yoyo_system_copywriting` WHERE `type`=@Type ORDER BY `key`", new { Type = type })).ToList();
            if (dbdata.Count > 0) { RedisCache.Set(CacheKey, dbdata, 120 * 60); }
            if (type.Equals("real_name_rule", StringComparison.OrdinalIgnoreCase))
            {
                result.Data = dbdata.FirstOrDefault().Text;
                return result;
            }
            result.Data = dbdata;
            return result;
        }

        public async Task<List<BaseTask>> GetBaseTask(int userId)
        {
            if (userId < 0) { return null; }
            StringBuilder Sql = new StringBuilder();
            Sql.AppendLine($"SELECT s.Id,s.TaskType,s.TaskTitle,s.TaskDesc,s.Reward,s.Aims,s.Unit,IFNULL(u.Carry,0) AS 'Carry',IFNULL(u.Completed,0) AS 'Completed' FROM yoyo_system_task AS s");
            Sql.AppendLine($"   LEFT JOIN (SELECT TaskId,Carry,Completed FROM yoyo_member_daily_task WHERE UserId={userId} AND CompleteDate=DATE_FORMAT(NOW() ,'%Y-%m-%d')) AS u ON s.Id=u.TaskId");
            Sql.AppendLine($"   WHERE s.`Status`=1");
            Sql.AppendLine($"   ORDER BY s.Sort");
            var Result = (await base.dbConnection.QueryAsync<BaseTask>(Sql.ToString())).ToList();
            if (Result.Count == 0) { Result = new List<BaseTask>(); }
            return Result;
        }

        public async Task<List<Ranking>> ShareRank(int type = 0)
        {
            var BaseSql = "SELECT UserId,ClickDate,COUNT(1) AS `ShareCount` FROM yoyo_ad_click WHERE ClickDate=DATE_FORMAT(NOW(),'%Y-%m-%d') GROUP BY UserId,ClickDate ORDER BY ShareCount DESC LIMIT 50";
            if (type == 1) { BaseSql = "SELECT UserId,DATE_FORMAT(ClickDate,'%Y-%m') AS `ClickDate`,COUNT(1) AS `ShareCount` FROM yoyo_ad_click WHERE DATE_FORMAT(ClickDate,'%Y-%m')=DATE_FORMAT(NOW(),'%Y-%m') GROUP BY UserId,DATE_FORMAT(ClickDate,'%Y-%m') ORDER BY ShareCount DESC LIMIT 50"; }
            StringBuilder Sql = new StringBuilder();
            Sql.AppendLine("SELECT u.`mobile` AS `Mobile`,u.`avatarUrl` AS `HeadImg`,u.`name` AS `Nick`,t.ShareCount AS `ShareCount` FROM (");
            Sql.AppendLine(BaseSql);
            Sql.AppendLine(") AS t INNER JOIN `user` AS u ON t.UserId=u.id");

            try
            {
                String FielUrl = "https://file.yoyoba.cn/";
                IEnumerable<Ranking> ListData = await base.dbConnection.QueryAsync<Ranking>(Sql.ToString());
                int i = 1;
                List<Ranking> rult = new List<Ranking>();
                foreach (var item in ListData)
                {
                    item.Rank = i;
                    item.HeadImg = FielUrl + item.HeadImg;
                    item.Mobile = Regex.Replace(item.Mobile, "(.{3})(.{4})(.{4})", "$1****$3");
                    rult.Add(item);
                    i++;
                }
                return rult;
            }
            catch
            {
                return new List<Ranking>();
            }
        }

        /// <summary>
        /// type==0 本日排行行  1 本月排行榜
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<List<Ranking>> Duplicate(int type = 0)
        {
            StringBuilder Sql = new StringBuilder();
            Sql.AppendLine("SELECT u.`mobile` AS `Mobile`,u.`avatarUrl` AS `HeadImg`,u.`name` AS `Nick`,d.`Duplicate` FROM (");
            if (type == 1)
            {
                Sql.AppendLine("SELECT `UserId`,SUM(`Duplicate`) AS `Duplicate` FROM yoyo_member_duplicate WHERE DATE_FORMAT(`Date`,'%Y-%m')=DATE_FORMAT(NOW(),'%Y-%m') GROUP BY `UserId` ORDER BY SUM(`Duplicate`) DESC");
            }
            else
            {
                Sql.AppendLine("SELECT `UserId`,`Duplicate` FROM yoyo_member_duplicate WHERE `Date`=DATE_FORMAT(NOW(),'%Y-%m-%d')");
            }
            Sql.AppendLine(") AS d INNER JOIN `user` AS u ON d.`UserId`=u.`Id` ORDER BY d.`Duplicate` DESC LIMIT 50");

            try
            {
                String FielUrl = "https://file.yoyoba.cn/";
                IEnumerable<Ranking> ListData = await base.dbConnection.QueryAsync<Ranking>(Sql.ToString());
                int i = 1;
                List<Ranking> rult = new List<Ranking>();
                foreach (var item in ListData)
                {
                    item.Rank = i;
                    item.HeadImg = FielUrl + item.HeadImg;
                    item.Mobile = Regex.Replace(item.Mobile, "(.{3})(.{4})(.{4})", "$1****$3");
                    rult.Add(item);
                    i++;
                }
                return rult;
            }
            catch
            {
                return new List<Ranking>();
            }
        }

        /// <summary>
        /// 推荐排行
        /// </summary>
        /// <param name="date">DateTime.Date</param>
        /// <returns></returns>
        public async Task<List<Ranking>> Recommend(GetRankingRequest request)
        {
            var date = DateTime.Now.Date;
            request.PageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            String FielUrl = "https://file.yoyoba.cn/";
            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParam = new DynamicParameters();

            QuerySql.AppendLine("SELECT u.`mobile` AS `Mobile`,u.`avatarUrl` AS `HeadImg`,u.`name` AS `Nick`,rank.InviteTotal,IF(rank.InviteDate<>@InviteDate,0,rank.InviteToday) AS `InviteDay` FROM ");
            QuerySql.AppendLine($"(SELECT * FROM yoyo_member_invite_ranking WHERE  Phase = @Phase AND InviteToday>=1 AND InviteDate=CURDATE() ORDER BY InviteToday DESC, Id DESC LIMIT {(request.PageIndex - 1) * request.PageSize},{request.PageSize}) AS rank ");
            QuerySql.AppendLine("INNER JOIN `user` AS u ON rank.UserId=u.id;");

            QueryParam.Add("Phase", date.Month, DbType.Int32);
            QueryParam.Add("InviteDate", date.Date, DbType.Date);

            try
            {
                IEnumerable<Ranking> ListData = await base.dbConnection.QueryAsync<Ranking>(QuerySql.ToString(), QueryParam);
                int i = 1;
                List<Ranking> rult = new List<Ranking>();
                foreach (var item in ListData)
                {
                    item.Rank = (request.PageIndex - 1) * request.PageSize + i;
                    item.HeadImg = FielUrl + item.HeadImg;
                    item.Mobile = Regex.Replace(item.Mobile, "(.{3})(.{4})(.{4})", "$1****$3");
                    rult.Add(item);
                    i++;
                }
                return rult;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("拉新活动==>>", ex);
                return new List<Ranking>();
            }
        }

        /// <summary>
        /// 轮播广告
        /// </summary>
        /// <param name="source"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        public MyResult<object> Banners(int source, long uid = 0)
        {
            MyResult result = new MyResult();
            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParam = new DynamicParameters();

            QuerySql.Append("SELECT ");
            QuerySql.Append("`id` AS `Id`, ");
            QuerySql.Append("`queue` AS `Queue`, ");
            QuerySql.Append("`title` AS `Title`, ");
            QuerySql.Append("CONCAT(@CosUrl,`imageUrl`) AS `ImageUrl`, ");
            QuerySql.Append("`type` AS `Type`, ");
            QuerySql.Append("`source` AS `Source`, ");
            QuerySql.Append("`status` AS `Status`, ");
            QuerySql.Append("`params` AS `Params`, ");
            QuerySql.Append("`cityCode` AS `CityCode`, ");
            QuerySql.Append("`createdAt` AS `CreatedAt` ");
            QuerySql.Append("FROM ");
            QuerySql.Append("sys_banner ");
            QuerySql.Append("WHERE source = @Source AND `status` = 1 ORDER BY `queue` ASC;");

            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);
            QueryParam.Add("Source", source, DbType.Int32);

            //var banner = $"SELECT * FROM sys_banner WHERE source={source} AND `status` = 1 ORDER BY `queue` DESC; ";
            //var bannerList = base.dbConnection.Query<SysBanner>(banner);

            List<SysBanner> bannerList = base.dbConnection.Query<SysBanner>(QuerySql.ToString(), QueryParam).ToList();
            if (uid > 0 && source == 2)
            {
                String CityNo = dbConnection.QueryFirstOrDefault<String>("SELECT `cityCode` FROM `user_locations` WHERE `userId` = @UserId;", new { UserId = uid });
                List<SysBanner> rult = new List<SysBanner>();
                if (!String.IsNullOrWhiteSpace(CityNo))
                {
                    rult = bannerList.Where(item => item.CityCode == CityNo).ToList();
                }
                if (rult.Count() > 0)
                {
                    result.Data = rult;
                    return result;
                }
            }
            result.Data = bannerList.Where(item => String.IsNullOrWhiteSpace(item.CityCode)).ToList();
            return result;
        }

        public MyResult<object> CandyRecord(BaseModel model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign error");
            }
            var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select freezeCandyNum,candyNum from user where id={userId}");
            var candyRecord = base.dbConnection.Query<GemRecords>($"select * from gem_records where userId = {userId} order by id desc").AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            if (userInfo == null)
            {
                userInfo = new User();
            }
            result.Data = new
            {
                CandyNum = userInfo.CandyNum,
                FreezeCandyNum = userInfo.FreezeCandyNum,
                CandyRecord = candyRecord
            };
            result.PageCount = pageCount;
            result.RecordCount = count;
            return result;
        }

        /// <summary>
        /// 果核流水
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public MyResult<object> CandyRecordH(BaseModel model, int userId, string mobile)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign error");
            }
            model.PageIndex = model.PageIndex == 0 ? 1 : model.PageIndex;
            model.PageSize = model.PageSize == 0 ? 10 : model.PageSize;
            //查询果核记录
            var candyHSql = $"select * from `user_candyp` where `userId`={userId} and `source`=3 order by id desc";
            candyHSql += $" limit {(model.PageIndex - 1) * model.PageSize},{model.PageSize}";
            var candyHLists = base.dbConnection.Query<UserCandyp>(candyHSql);
            var candyHCountSql = $"select count(id) count from `user_candyp` where `userId`={userId} and `source`=3";
            var candyHCount = base.dbConnection.QueryFirstOrDefault<int>(candyHCountSql);

            //计算加成果核 已经 基础果核
            StringBuilder QueryTaskIds = new StringBuilder();
            QueryTaskIds.Append("SELECT `minningId` FROM `minnings` WHERE `userId` = @UserId AND `status` = 1 ");
            QueryTaskIds.Append("AND `beginTime` < Now() AND `endTime` > Now();");
            DynamicParameters QueryTaskIdsParam = new DynamicParameters();
            QueryTaskIdsParam.Add("UserId", userId, DbType.Int32);
            List<Int32> minningIds = base.dbConnection.Query<Int32>(QueryTaskIds.ToString(), QueryTaskIdsParam).ToList();
            decimal baseCandyH = 0;
            decimal extCandyH = 0;
            decimal MaxTaskCandy = 0M;//最高矿机  按投入糖果数
            decimal TaskCandy = 0M;

            minningIds.ToList().ForEach(minningId =>
            {
                TaskCandy = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyIn;
                baseCandyH += Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyH;
                if (TaskCandy > MaxTaskCandy)
                {
                    MaxTaskCandy = TaskCandy;
                }
            });

            result.PageCount = candyHCount / model.PageSize;
            result.RecordCount = candyHCount;

            #region 更改多次执行SQL为单次执行，更改使用手机进行下级查询改为用ID进行。
            StringBuilder QuerySubTaskIds = new StringBuilder();
            QuerySubTaskIds.Append("SELECT `minningId`,`beginTime` FROM `minnings` WHERE `status`=1 AND `beginTime` < Now() AND `endTime` > Now() AND `userId` IN ( ");
            QuerySubTaskIds.Append("SELECT id FROM `user` WHERE `auditState`=2 AND `status`=0 AND `inviterMobile`= @UserMobile );");
            DynamicParameters QuerySubTaskIdsParam = new DynamicParameters();
            QuerySubTaskIdsParam.Add("UserMobile", mobile, DbType.String);

            List<TaskInfo> _SubminningIds = base.dbConnection.Query<TaskInfo>(QuerySubTaskIds.ToString(), QuerySubTaskIdsParam).ToList();
            DateTime BeginTime = new DateTime(2020, 03, 16);
            Decimal BonRate = 1.00M;
            try
            {
                String CacheKeyBon = $"UserBon";
                if (RedisCache.HExists(CacheKeyBon, userId.ToString()))
                {
                    BonRate = RedisCache.HGet<Decimal>(CacheKeyBon, userId.ToString());
                }
            }
            catch { }
            _SubminningIds.ForEach(_minning =>
            {
                Decimal TmpextH = 0;
                if (Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).CandyIn <= MaxTaskCandy || _minning.MinningId == 6 || _minning.MinningId == 16)
                {
                    if (_minning.MinningId == 6 || _minning.MinningId == 16)
                    {
                        TmpextH = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).CandyH * 0.10M * BonRate;
                    }
                    else
                    {
                        TmpextH = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).CandyH * 0.05M;
                    }
                    extCandyH += TmpextH;
                }
            });
            #endregion

            result.Data = new
            {
                BaseCandyH = baseCandyH,
                ExtCandyH = extCandyH,
                CandyHLists = candyHLists
            };
            return result;
        }

        public MyResult<object> CandyRecordP(BaseModel model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign error");
            }
            //查询果皮记录
            var candyPLists = base.dbConnection.Query<UserCandyp>($"select * from `user_candyp` where `userId`={userId} and `source` != 3 order by id desc").AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = candyPLists;
            return result;
        }
        //客户端下载链接
        public MyResult<object> ClientDownloadUrl(string systemName)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(systemName))
            {
                return result.SetStatus(ErrorCode.InvalidData, "系统设备异常");
            }
            var sysClientVersion = base.dbConnection.QueryFirstOrDefault<SysClientVersions>($"select * from `sys_client_versions` where `deviceSystem`=@systemName order by id desc limit 1", new { systemName });
            result.Data = sysClientVersion;
            return result;
        }

        /// <summary>
        /// 加速矿机
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> QuickenTask(Int64 UserId)
        {
            MyResult<Object> result = new MyResult<Object>();
            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "请重新登陆"); }
            String QuickenLockStr = $"QuickenTask:{UserId}";
            if (RedisCache.Exists(QuickenLockStr))
            {
                return result.SetStatus(ErrorCode.InvalidData, "操作过于频繁，请稍后重试");
            }
            RedisCache.Set(QuickenLockStr, UserId, 15);

            try
            {
                StringBuilder QueryScheduleSql = new StringBuilder();
                QueryScheduleSql.Append("SELECT * FROM yoyo_task_record WHERE UserId = @UserId AND `CreateDate` = DATE(NOW());");
                YoyoTaskRecord TaskSchedule = await dbConnection.QueryFirstOrDefaultAsync<YoyoTaskRecord>(QueryScheduleSql.ToString(), new { UserId = UserId });
                if (TaskSchedule == null) { return result.SetStatus(ErrorCode.InvalidData, "矿机未开始"); }
                if (((DateTime)TaskSchedule.EndTime).AddMinutes(-AppSetting.TaskQuicken * (Int32)TaskSchedule.Schedule) < DateTime.Now) { return result.SetStatus(ErrorCode.InvalidData, "快去领取奖励吧"); }

                Int32 Rows = dbConnection.Execute("UPDATE `yoyo_task_record` SET `UpdateDate` = @UpdateDate, `Schedule` = `Schedule` + 1 WHERE UserId = @UserId AND `CreateDate` = DATE(NOW());",
                    new { UserId = UserId, UpdateDate = DateTime.Now });

                if (Rows > 0)
                {
                    TaskSchedule.Schedule = TaskSchedule.Schedule + 1;
                    result.Data = new
                    {
                        TaskSchedule.StartTime,
                        TaskSchedule.EndTime,
                        QuickenMinutes = AppSetting.TaskQuicken * TaskSchedule.Schedule
                    };
                    return result;
                }
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("矿机加速失败", ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "矿机加速失败");
        }

        /// <summary>
        /// 开始矿机
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> DoTask(int userId, string mobile)
        {
            MyResult result = new MyResult();
            if (userId < 0) { return result.SetStatus(ErrorCode.InvalidToken, "sign error"); }

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"DoTask:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 基础验证 及 矿机进度
                User UserInfo = base.dbConnection.QueryFirstOrDefault<User>($"SELECT `status`, auditState, `level`, `name`, candyNum, inviterMobile, ctime FROM `user` WHERE id={userId};");
                if (UserInfo.Status != 0) { return result.SetStatus(ErrorCode.SystemError, "账号状态异常"); }
                if (UserInfo.AuditState != 2) { return result.SetStatus(ErrorCode.SystemError, "未实名不能做矿机!"); }
                if (UserInfo.CandyNum < 0) { return result.SetStatus(ErrorCode.SystemError, "糖果账户异常"); }

                YoyoTaskRecord TaskRecord = dbConnection.QueryFirstOrDefault<YoyoTaskRecord>("SELECT * FROM yoyo_task_record WHERE UserId = @UserId AND Source = 0 AND `CreateDate` = DATE(NOW());",
                    new { UserId = userId });

                if (TaskRecord == null)
                {
                    DateTime EndTime = DateTime.Now.AddHours(3);
                    dbConnection.Execute("INSERT INTO `yoyo_task_record`(`UserId`, `Schedule`, `Source`, `CreateDate`, `StartTime`, `EndTime`, `UpdateDate`) VALUES (@UserId, 0, 0, DATE(NOW()), NOW(), @EndTime , NOW());",
                        new { UserId = userId, EndTime = EndTime });

                    result.Data = new { StartTime = DateTime.Now, EndTime = EndTime, QuickenMinutes = 0 };
                    return result;
                }
                if (TaskRecord.EndTime > DateTime.Now)
                {
                    Double TaskTime = ((DateTime)TaskRecord.EndTime - DateTime.Now).TotalSeconds - ((Int32)TaskRecord.Schedule * AppSetting.TaskQuicken * 60);

                    if (TaskTime > 0)
                    {
                        return result.SetStatus(ErrorCode.SystemError, "正在努力挖矿中...");
                    }
                }
                #endregion

                Int64 ReferrerId = base.dbConnection.QueryFirstOrDefault<Int64>("SELECT id FROM `user` WHERE mobile = @Mobile;", new { Mobile = UserInfo.InviterMobile });

                //查询今日是否做矿机
                var isDoTask = base.dbConnection.QueryFirstOrDefault<decimal>($"select IFNULL(`num`,0) num from `gem_records` where userId={userId} and gemSource=1 and TO_DAYS(`createdAt`)=TO_DAYS(now())");
                if (isDoTask != 0) { return result.SetStatus(ErrorCode.TaskHadDo, "今日矿机已完成"); }

                //查询基础矿机产量 查询下级附加果核产量 1个附加果核 多产一个
                #region 计算加成日产量 已经 基础日产量
                StringBuilder QueryTaskIds = new StringBuilder();
                QueryTaskIds.Append("SELECT `minningId` FROM `minnings` WHERE `userId` = @UserId AND `status` = 1 ");
                QueryTaskIds.Append("AND `beginTime` < Now() AND `endTime` > Now();");
                DynamicParameters QueryTaskIdsParam = new DynamicParameters();
                QueryTaskIdsParam.Add("UserId", userId, DbType.Int32);
                List<Int32> minningIds = base.dbConnection.Query<Int32>(QueryTaskIds.ToString(), QueryTaskIdsParam).ToList();

                if (minningIds.Count() == 0)
                {
                    return result.SetStatus(ErrorCode.TaskHadDo, "当前矿机已过期，快到矿机商店兑换新矿机吧");
                }
                #endregion

                decimal DayCandyOut = 0;    //我的矿机产量
                decimal extDayCandyOut = 0; //下级加成 糖果数
                decimal MaxTaskCandy = 0M;  //最高矿机  按投入糖果数
                decimal TaskCandy = 0M;     //矿机投入糖果数  临时用
                decimal DelTaskCandy = 0.0000M;  //烧伤

                #region 上级 拉新1个月  加成

                //if (UserInfo.Ctime > DateTime.Parse("2020-07-10") && UserInfo.Ctime.AddDays(30).Date > DateTime.Now.Date && ReferrerId > 0)
                //{
                //    List<Int32> ReferrerTasks = base.dbConnection.Query<Int32>("SELECT `minningId` FROM `minnings` WHERE `userId` = @ReferrerId AND `status` = 1 AND `beginTime` < Now() AND `endTime` > Now();", new { ReferrerId }).ToList();

                //    if (ReferrerTasks.Count > 0)
                //    {
                //        ReferrerTasks.ForEach(TaskId =>
                //        {
                //            decimal TempTaskCandy = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == TaskId).CandyIn;
                //            if (TempTaskCandy > NewMaxTaskCandy)
                //            {
                //                NewMaxTaskCandy = TempTaskCandy;
                //            }
                //        });
                //    }
                //}
                #endregion

                minningIds.ToList().ForEach(minningId =>
                {
                    TaskCandy = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyIn;
                    DayCandyOut += Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).DayCandyOut;
                    if (TaskCandy > MaxTaskCandy)
                    {
                        MaxTaskCandy = TaskCandy;
                    }
                    #region 上级 拉新1个月  加成   (2020-08-10取消)
                    //if (NewMaxTaskCandy >= TaskCandy && NewMaxTaskCandy > 0)
                    //{
                    //    NewTaskCandy += Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).DayCandyOut;
                    //}
                    //else
                    //{
                    //    NewDelTaskCandy += Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).DayCandyOut;
                    //}
                    #endregion
                });

                if (DayCandyOut == 0)
                {
                    return result.SetStatus(ErrorCode.TaskHadDo, "矿机未完成，未获得糖果");
                }

                #region 下级矿机加成  加烧商  2020-08-10版
                StringBuilder QuerySubTaskIds = new StringBuilder();  //查下级所有矿机
                QuerySubTaskIds.Append("SELECT `minningId`, `beginTime`, `userId` FROM `minnings` WHERE `status`=1 AND `beginTime` < Now() AND `endTime` > Now() AND `userId` IN ( ");
                QuerySubTaskIds.Append("SELECT id FROM `user` WHERE `auditState`=2 AND `status`=0 AND `inviterMobile`= @UserMobile );");
                DynamicParameters QuerySubTaskIdsParam = new DynamicParameters();
                QuerySubTaskIdsParam.Add("UserMobile", mobile, DbType.String);
                List<TaskInfo> _SubminningIds = base.dbConnection.Query<TaskInfo>(QuerySubTaskIds.ToString(), QuerySubTaskIdsParam).ToList();
                List<Int64> SubUserIds = _SubminningIds.GroupBy(item => item.UserId).Select(item => item.Key).ToList();

                List<Int64> SubActiveUids = new List<Int64>();
                if (SubUserIds.Count() > 0)
                {
                    StringBuilder QueryActiveSql = new StringBuilder(); //查下级所有昨天做矿机的人
                    QueryActiveSql.Append("SELECT userId FROM gem_records WHERE gemSource = 1 AND TO_DAYS(createdAt) = TO_DAYS(DATE_ADD(NOW(),INTERVAL -1 DAY)) AND userId IN (");
                    QueryActiveSql.Append(String.Join(",", SubUserIds)).Append(");");
                    SubActiveUids = base.dbConnection.Query<Int64>(QueryActiveSql.ToString()).ToList();
                }

                Decimal BonRate = 1.00M;
                try
                {
                    String CacheKeyBon = $"UserBon";
                    if (RedisCache.HExists(CacheKeyBon, userId.ToString()))
                    {
                        BonRate = RedisCache.HGet<Decimal>(CacheKeyBon, userId.ToString());
                    }
                }
                catch { }

                #region 新版烧伤 2020-08-10
                foreach (Int64 SubUid in SubUserIds)
                {
                    Decimal SubTaskCandy = 0.00M;
                    foreach (TaskInfo SubTasks in _SubminningIds.Where(item => item.UserId == SubUid))
                    {
                        Tasks SysTaskInfo = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == SubTasks.MinningId);
                        if (SysTaskInfo.CandyIn <= 0 && SysTaskInfo.MinningId != 20 && SysTaskInfo.MinningId != 16) { continue; }
                        SubTaskCandy += SysTaskInfo.DayCandyOut;
                    }
                    if (SubActiveUids.Where(item => item == SubUid).Count() > 0)
                    {
                        if (SubTaskCandy > DayCandyOut)
                        {
                            extDayCandyOut += DayCandyOut * 0.05M;
                            DelTaskCandy += (SubTaskCandy - DayCandyOut) * 0.05M;
                        }
                        else { extDayCandyOut += SubTaskCandy * 0.05M; }
                    }
                    else { DelTaskCandy += SubTaskCandy * 0.05M; }
                }
                #endregion

                #region 2020-08-10 前 旧版本
                //_SubminningIds.ForEach(_minning =>
                //{
                //    Decimal TmpextOut = 0;
                //    if (Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).CandyIn <= MaxTaskCandy || _minning.MinningId == 6 || _minning.MinningId == 16)
                //    {
                //        if (_minning.MinningId == 6 || _minning.MinningId == 16)
                //        {
                //            TmpextOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).DayCandyOut * 0.10M * BonRate;
                //        }
                //        else
                //        {
                //            TmpextOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).DayCandyOut * 0.05M;
                //        }
                //        extDayCandyOut += TmpextOut;
                //    }
                //    else if (Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).CandyIn > 0)
                //    {
                //        DelTaskCandy += Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning.MinningId).DayCandyOut * 0.05M;
                //    }
                //});
                #endregion
                #endregion

                #region 验证城市合伙人
                decimal CityCandyOut = 0;
                //List<int> TaskUp = new List<int> { 101, 102, 103, 104, 105 };
                List<int> TaskUp = Constants.MinningListSetting
                    .Where(item => item.CandyIn >= 100 && item.CandyIn <= 10000)
                    .Select(item => item.MinningId).ToList();

                minningIds.Where(o => TaskUp.Contains(o)).ToList().ForEach(_minning =>
                {
                    Decimal TmpextOut = 0;
                    TmpextOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == _minning).DayCandyOut * 0.01M;
                    CityCandyOut += TmpextOut;
                });
                long CityMaster = 0;
                string CityCode = base.dbConnection.ExecuteScalar<string>($"SELECT IFNULL(`CityCode`,'') AS `CityCode` FROM `user_locations` WHERE userId={userId} LIMIT 1");
                if (!String.IsNullOrWhiteSpace(CityCode))
                {
                    long? CityUserId = base.dbConnection.ExecuteScalar<long?>($"SELECT UserId FROM `yoyo_city_master` WHERE `CityCode`='{CityCode}' AND `StartTime`<=CURDATE() AND `EndTime`>=CURDATE()");
                    if (CityUserId != null)
                    {
                        // 账户为负数  不获得 加成
                        Decimal CityCandy = base.dbConnection.QueryFirstOrDefault<Decimal?>($"SELECT candyNum FROM `user` WHERE id={CityUserId};") ?? -1;
                        if (CityCandy >= 0)
                        {
                            CityMaster = CityUserId.Value;
                        }
                    }
                }
                #endregion

                var totalCandyToday = DayCandyOut + extDayCandyOut;
                //更新用户糖果
                //写入记录
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        base.dbConnection.Execute($"update user set candyNum = (candyNum + {totalCandyToday}) where id = {userId}", null, transaction);
                        var insertSql = $"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},{DayCandyOut},'基础矿机奖励',1)";
                        base.dbConnection.Execute(insertSql, null, transaction);
                        if (extDayCandyOut >= 0) //如果下级以后加成，那么写入加成记录
                        {
                            base.dbConnection.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},{extDayCandyOut},'下级加成奖励:{extDayCandyOut.ToString("0.0000")}颗,烧伤:{DelTaskCandy.ToString("0.0000")}颗',2)", null, transaction);
                        }

                        #region 城市合伙人矿机加成
                        if (CityMaster > 0 && CityCandyOut > 0 && userId != CityMaster)
                        {
                            base.dbConnection.Execute($"update user set candyNum = (candyNum + {CityCandyOut}) where id = {CityMaster}", null, transaction);
                            base.dbConnection.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({CityMaster},{CityCandyOut},'[城内用户{UserInfo.Name}]矿机奖励',51)", null, transaction);
                        }
                        #endregion

                        #region 上级 拉新1个月  加成
                        //上级 拉新1个月  加成
                        //if (NewTaskCandy > 0)
                        //{
                        //    Decimal PullRate = 0.0500M;
                        //    base.dbConnection.Execute("UPDATE `user` SET candyNum = candyNum + @CandyNum WHERE id = @ReferrerId;", new { CandyNum = NewTaskCandy * PullRate, ReferrerId }, transaction);
                        //    base.dbConnection.Execute("INSERT INTO `gem_records` ( `userId`, `num`, `description`, gemSource ) VALUES ( @UserId, @CandyNum, @Desc, @Source)",
                        //        new { UserId = ReferrerId, CandyNum = NewTaskCandy * PullRate, Desc = $"下级[{UserInfo.Name}]矿机奖励:{(NewTaskCandy * PullRate).ToString("0.0000")}颗,烧伤:{(NewDelTaskCandy * PullRate).ToString("0.0000")}颗", Source = 8 }, transaction);
                        //}
                        #endregion

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Yoyo.Core.SystemLog.Debug($"{userId}做矿机 => 事务异常：", ex);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();

                result.Data = new { TaskSchedule = 10000 };

                //RedisCache.Set(CacheKey, totalCandyToday, (int)(DateTime.Now.Date.AddDays(1) - DateTime.Now).TotalSeconds);
                //String PushStr = new { UserId = userId, JPushId = "", Remark = "做矿机矿机" }.GetJson();
                //RedisCache.Publish("YoYo_Member_Active", PushStr);

                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("做矿机 => 异常：", ex);
                return result.SetStatus(ErrorCode.SystemError, "做矿机失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 兑换矿机
        /// </summary>
        /// <param name="minningId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> Exchange(int minningId, int userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign token");
            }
            #region 兑换矿机验证
            var minningInfo = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId);
            if (minningInfo == null)
            {
                return result.SetStatus(ErrorCode.Forbidden, "矿机不存在");
            }
            if (minningId <= 0) { return result.SetStatus(ErrorCode.InvalidData, "矿机类型有误"); }
            if (!minningInfo.IsExchange) { return result.SetStatus(ErrorCode.InvalidData, "该矿机不支持兑换..."); }
            StringBuilder QueryTaskInfoSql = new StringBuilder();

            if (AppSetting.TaskLimitIds.Contains(minningId) && AppSetting.TaskLimit > 0)
            {
                Int32 TodayCunt = dbConnection.QueryFirstOrDefault<Int32>("SELECT COUNT(id) FROM minnings WHERE createdAt > DATE_FORMAT(NOW(),'%Y-%m-%d') AND source = 1 AND minningId = @TaskId;",
                    new { TaskId = minningId });
                if (TodayCunt >= AppSetting.TaskLimit) { return result.SetStatus(ErrorCode.InvalidData, "今日已兑完,明天早点哦~"); }
            }

            #endregion

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"Exchange:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 获取会员信息
                StringBuilder QueryUserSql = new StringBuilder();
                DynamicParameters QueryUserParam = new DynamicParameters();
                QueryUserSql.Append("SELECT id, `status`, auditState, `name`, candyNum, candyP, inviterMobile, ctime, `level` FROM `user` WHERE id = @UserId;");
                QueryUserParam.Add("UserId", userId, DbType.Int32);
                User user = base.dbConnection.QueryFirstOrDefault<User>(QueryUserSql.ToString(), QueryUserParam);
                if (user == null || user.Status != 0) { return result.SetStatus(ErrorCode.Forbidden, "账号异常请联系客服"); }
                if (user.AuditState != 2) { return result.SetStatus(ErrorCode.Forbidden, "未实名不能兑换矿机..."); }
                #endregion

                Int32 candyIn = 0;
                try
                {
                    candyIn = minningInfo.CandyIn;
                    if (candyIn <= 0)
                    {
                        return result.SetStatus(ErrorCode.Forbidden, "此矿机不能兑换...");
                    }
                }
                catch (System.Exception ex)
                {
                    LogUtil<SystemService>.Error(ex, "兑换矿机时发生异常");
                    return result.SetStatus(ErrorCode.Forbidden, "矿机类型错误");
                }
                //余额
                var userAccountWallet = await base.dbConnection.QueryFirstOrDefaultAsync<decimal>($"select Balance from `user_account_wallet` where UserId={userId} and `coinType`='LF'");
                if (userAccountWallet < minningInfo.CandyIn)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "余额不足...");
                }
                #region 矿机上限
                var taskCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(*) as count from minnings where userId = {userId} and endTime > now() and minningId = {minningId} AND source = 1;");

                if (taskCount >= minningInfo.MaxHave)
                {
                    return result.SetStatus(ErrorCode.Forbidden, "该类型矿机超出上限");
                }
                #endregion

                #region 计算 矿机周期
                Int32 Days = 30;
                if (!Int32.TryParse(minningInfo.RunTime.Replace("天", ""), out Days))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "兑换矿机失败");
                }
                var effectiveBiginTime = DateTime.Now.Date.AddDays(1).ToLocalTime().ToString("yyyy-MM-dd");
                var effectiveEndTime = DateTime.Now.Date.AddDays(Days + 1).ToLocalTime().ToString("yyyy-MM-dd");
                #endregion

                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        base.dbConnection.Execute($"insert into minnings (userId, minningId, beginTime, endTime, source) values ({userId}, {minningId},'{effectiveBiginTime}' , '{effectiveEndTime}',1)", null, transaction);
                        var _candyIn = (decimal)minningInfo.CandyIn;
                        var res3 = await ChangeWalletAmount(transaction, true, userId, "LF", -_candyIn, LfexCoinnModifyType.Exchange_Minner, false, minningInfo.MinningName, _candyIn.ToString());
                        if (res3.Code != 200)
                        {
                            return result.SetStatus(ErrorCode.SystemError, res3.Message);
                        }
                        //发消息
                        var c1Num = Math.Round(_candyIn, 4);
                        var c1 = RedisCache.Publish("Lfex_Member_LFChange_Signle", JsonConvert.SerializeObject(new { bId = userId, bBalance = -c1Num }));

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Yoyo.Core.SystemLog.Debug("矿机兑换 => 事务异常：", ex);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();
                result.Data = true;
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("矿机兑换 => 异常：", ex);
                return result.SetStatus(ErrorCode.SystemError, "矿机兑换失败");
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
        /// 矿机续期
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> TaskRenew(int taskId, int userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign token");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"TaskRenew:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 获取会员信息
                StringBuilder QueryUserSql = new StringBuilder();
                DynamicParameters QueryUserParam = new DynamicParameters();
                QueryUserSql.Append("SELECT id, `status`, auditState, `name`, candyNum, candyP, inviterMobile, ctime, `level` FROM `user` WHERE id = @UserId;");
                QueryUserParam.Add("UserId", userId, DbType.Int32);
                User user = base.dbConnection.QueryFirstOrDefault<User>(QueryUserSql.ToString(), QueryUserParam);
                if (user == null || user.Status != 0) { return result.SetStatus(ErrorCode.Forbidden, "账号异常请联系客服"); }
                if (user.AuditState != 2) { return result.SetStatus(ErrorCode.Forbidden, "请先完成实名信息"); }
                #endregion

                //查询现有矿机状态
                var task = base.dbConnection.QueryFirstOrDefault<Minnings>($"select * from minnings where userId = {userId} and id={taskId}");
                if (null == task) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }
                if (task.Status.Value == 0) { return result.SetStatus(ErrorCode.InvalidData, "矿机已过期，无法续期"); }
                if (task.EndTime < DateTime.Now) { return result.SetStatus(ErrorCode.InvalidData, "矿机已过期，无法续期"); }


                //不可续期矿机
                if (!Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).IsRenew) { return result.SetStatus(ErrorCode.InvalidData, "此矿机不可以续期"); }
                if (task.Source != 1 && task.Source != 11) { return result.SetStatus(ErrorCode.InvalidData, "系统赠送矿机不可以续期"); }
                var candyIn = 0;
                try
                {
                    candyIn = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).CandyIn;
                }
                catch (System.Exception ex)
                {
                    LogUtil<SystemService>.Error(ex, "矿机续期是发生异常");
                    return result.SetStatus(ErrorCode.Forbidden, "矿机类型错误");
                }
                if (user.CandyNum < candyIn)
                {
                    return result.SetStatus(ErrorCode.Forbidden, "账户糖果不足");
                }

                #region 获取用户等级信息
                var SysLevel = AppSetting.Levels.FirstOrDefault(o => o.Level.ToLower().Equals(user.Level.ToLower()));
                if (null == SysLevel) { return result.SetStatus(ErrorCode.Forbidden, "矿机续期异常，请联系管理员。"); }
                #endregion
                //=====计算用户兑换矿机获得果皮数量
                decimal GiveUserCandyP = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).CandyP * SysLevel.ExchangeRate;

                //1 新增矿机 2 更新用户糖果 果皮 3 增加果皮流水 增加糖果流水
                Int32 Days = 30;
                Tasks TaskInfo = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId);
                if (!Int32.TryParse(TaskInfo.RunTime.Replace("天", ""), out Days))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "矿机续期失败");
                }
                var effectiveEndTime = task.EndTime.AddDays(Days).ToLocalTime().ToString("yyyy-MM-dd");
                if ((task.EndTime.AddDays(Days) - task.BeginTime).TotalDays > 180)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "矿机续期时间，超出限制");
                }
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        base.dbConnection.Execute($"UPDATE `minnings` SET endTime='{effectiveEndTime}',`status`=1 WHERE userId={userId} AND id={task.Id}", null, transaction);
                        base.dbConnection.Execute($"update user set candyNum = (candyNum + {-candyIn}),candyP=(candyP+{GiveUserCandyP}) where id = {userId}", null, transaction);
                        base.dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},{GiveUserCandyP},'矿机续期,赠送{GiveUserCandyP}果皮',4,now(),now())", null, transaction);
                        base.dbConnection.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},{-candyIn},'矿机续期消耗" + candyIn + "糖果',4)", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug("矿机续期", ex);
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();

                //发消息
                try
                {

                    var c = RedisCache.Publish("YoYo_Member_TaskAction", JsonConvert.SerializeObject(new { MemberId = userId, TaskLevel = task.MinningId, Devote = candyIn, RenewTask = true }));
                    if (c == 0)
                    {
                        LogUtil<YoyoUserSerivce>.Error("YoYo_Member_TaskAction c 消息返送失败");
                    }
                }
                catch (System.Exception)
                {
                }
                result.Data = true;
                return result;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.SystemError, "矿机续期失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 系统消息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> Notices(NoticesDto model, int userId)
        {
            MyResult result = new MyResult();

            var notice = $"SELECT * FROM notice_infos WHERE type={model.Type}";
            if (model.Type == 1)
            {
                notice += $" and userId={userId}";
            }
            notice += $" order by id desc";
            var noticeList = base.dbConnection.Query<NoticeInfos>(notice).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = noticeList;
            return result;
        }

        public MyResult<object> OneNotice()
        {
            MyResult result = new MyResult();
            var noticeSql = $"SELECT * FROM notice_infos WHERE type=0 order by id desc limit 1";
            var notice = base.dbConnection.QueryFirstOrDefault<NoticeInfos>(noticeSql);
            result.Data = notice;
            return result;
        }

        public MyResult<object> TaskList(int type, int userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "sign error");
            }
            IEnumerable<Minnings> task;
            if (type == 0)
            {
                task = base.dbConnection.Query<Minnings>($"select * from minnings where userId = {userId} and endTime > now() and date_sub(beginTime, interval 1 day) <= now() order by id");

            }
            else if (type == 1)
            {
                task = base.dbConnection.Query<Minnings>($"select * from minnings where userId = {userId} and endTime < now() order by id");
            }
            else
            {
                return result.SetStatus(ErrorCode.InvalidData, "类型错误");
            }
            List<MinningDto> minningDtoList = new List<MinningDto>();
            //名称过滤重组
            task.ToList().ForEach(t =>
            {
                MinningDto minningDto = new MinningDto();
                minningDto.MinningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).MinningName;
                minningDto.CandyIn = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).CandyIn;
                minningDto.CandyOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).CandyOut;
                minningDto.RunTime = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).RunTime;
                minningDto.CandyH = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).CandyH;
                minningDto.CandyP = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).CandyP;
                minningDto.CandyP = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).CandyP;
                minningDto.DayCandyOut = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).DayCandyOut;
                minningDto.MaxHave = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).MaxHave;
                minningDto.Colors = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).Colors;
                minningDto.Id = t.Id;
                minningDto.UserId = t.UserId;
                minningDto.MinningId = t.MinningId;
                minningDto.BeginTime = t.BeginTime;
                minningDto.EndTime = t.EndTime;
                minningDto.Status = t.Status;
                minningDto.CreatedAt = t.CreatedAt;
                minningDto.UpdatedAt = t.UpdatedAt;
                minningDto.WorkingTime = t.WorkingTime;
                minningDto.Source = t.Source;
                minningDtoList.Add(minningDto);
            });
            result.Data = minningDtoList;
            return result;
        }

        public async Task<MyResult<object>> TasksShop(Int64 UserId, int status)
        {
            MyResult result = new MyResult();
            if (status == 0)
            {
                List<Tasks> TaskList = new List<Tasks>();
                TaskList = Constants.MinningListSetting.Where(item => item.MinningId > 0 && item.StoreShow).OrderBy(item => item.CandyIn).ThenBy(item => item.MinningId).ToList();
                result.Data = TaskList;
            }
            else
            {
                IEnumerable<Minnings> task;
                task = await base.dbConnection.QueryAsync<Minnings>($"select * from minnings where userId = {UserId} and minningId>1 and endTime > now() order by id");

                List<domain.models.lfexDto.MinningDto> minningDtoList = new List<domain.models.lfexDto.MinningDto>();
                //名称过滤重组
                task.ToList().ForEach(t =>
                {
                    domain.models.lfexDto.MinningDto minningDto = new domain.models.lfexDto.MinningDto();
                    minningDto.MinningName = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == t.MinningId).MinningName;
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
            }
            return result;
        }

        public MyResult<object> TodayTask(int source)
        {
            MyResult result = new MyResult();

            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParam = new DynamicParameters();

            QuerySql.Append("SELECT ");
            QuerySql.Append("`id` AS `Id`, ");
            QuerySql.Append("`queue` AS `Queue`, ");
            QuerySql.Append("`title` AS `Title`, ");
            QuerySql.Append("CONCAT(@CosUrl,`imageUrl`) AS `ImageUrl`, ");
            QuerySql.Append("`type` AS `Type`, ");
            QuerySql.Append("`source` AS `Source`, ");
            QuerySql.Append("`status` AS `Status`, ");
            QuerySql.Append("`params` AS `Params`, ");
            QuerySql.Append("`createdAt` AS `CreatedAt` ");
            QuerySql.Append("FROM ");
            QuerySql.Append("sys_banner ");
            QuerySql.Append("WHERE source = @Source AND `status` = 1 ORDER BY `queue` ASC;");

            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);
            QueryParam.Add("Source", source, DbType.Int32);

            SysBanner bannerModel = base.dbConnection.QueryFirstOrDefault<SysBanner>(QuerySql.ToString(), QueryParam);

            result.Data = bannerModel;
            return result;
        }

        /// <summary>
        /// 获取用户关键信息
        /// </summary>
        /// <returns></returns>
        public MyResult<UserReturnDto> UserInfo(int userId)
        {
            MyResult<UserReturnDto> result = new MyResult<UserReturnDto>();
            if (userId <= 0)
            {
                result.Data = new UserReturnDto();
                return result;
            }
            var userInfoSql = $"select a.*,au.`failReason` from (select u.id,u.`alipay`,u.`alipayPic`,u.`level`,u.`uuid` adress,u.`rcode`,u.inviterMobile,u.`status`,u.`auditState`,u.`golds` as golds,u.mobile,u.candyNum,u.`candyP`,IFNULL(og.`status`,0) isPay from user u left join `order_games` og on u.id=og.userId where u.id={userId}) a left join `authentication_infos` au on a.id=au.userId";
            var userInfo = base.dbConnection.QueryFirstOrDefault<UserReturnDto>(userInfoSql);
            if (userInfo == null)
            {
                result.Data = new UserReturnDto();
            }
            else
            {
                #region 获取联系方式
                ContactModel ReContact = base.dbConnection.QueryFirstOrDefault<ContactModel>("SELECT * FROM `user_expand` WHERE `UserId` = (SELECT id FROM `user` WHERE mobile = @InviterMobile);", new { InviterMobile = userInfo.inviterMobile });
                userInfo.ReContactTel = ReContact?.Mobile ?? String.Empty;
                userInfo.ReWeChatNo = ReContact?.WeChat ?? String.Empty;

                ContactModel MyContact = base.dbConnection.QueryFirstOrDefault<ContactModel>("SELECT * FROM `user_expand` WHERE `UserId` = @UserId;", new { UserId = userId });
                userInfo.MyContactTel = MyContact?.Mobile ?? String.Empty;
                userInfo.MyWeChatNo = MyContact?.WeChat ?? String.Empty;
                #endregion

                if (userInfo.Status != 0) { return result.SetStatus(ErrorCode.ReLogin, "该账户已被封禁,请联系管理员"); }
                userInfo.AlipayUid = String.IsNullOrWhiteSpace(userInfo.AlipayUid) ? "" : userInfo.AlipayUid;
                userInfo.IsPay = userInfo.IsPay == 1 ? 1 : 0;
                userInfo.Rcode = userInfo.Rcode == null ? "0" : userInfo.Rcode;

                #region 定值矿机数据
                userInfo.AdInterval = AppSetting.AdInterval;
                userInfo.PullUpTimes = AppSetting.PullUpTimes;
                userInfo.AuthAdCount = AppSetting.AuthAdCount;
                #endregion
                if (!String.IsNullOrWhiteSpace(userInfo.Level)) { userInfo.Level = userInfo.Level.ToUpper(); }
                if (!string.IsNullOrWhiteSpace(userInfo.AlipayPic))
                {
                    userInfo.AlipayPic = Constants.CosUrl + userInfo.AlipayPic;
                }
                result.Data = userInfo;
            }
            return result;
        }


        #region 后台管理
        /// <summary>
        /// 添加会员矿机
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public MyResult<object> AddUserTask(QuerySysTaskModel query)
        {
            Int64 userId = query.UserId;
            Int32 minningId = query.TaskId;

            MyResult<object> rult = new MyResult<object>();
            if (minningId < 50) { return rult.SetStatus(ErrorCode.InvalidData, "此矿机已禁止添加"); }
            if (userId <= 0 && string.IsNullOrWhiteSpace(query.Mobile)) { return rult.SetStatus(ErrorCode.InvalidData, "会员编号或手机号必填一项"); }

            #region 获取会员信息
            StringBuilder QueryUserSql = new StringBuilder();
            DynamicParameters QueryUserParam = new DynamicParameters();
            QueryUserSql.Append("SELECT id, `status`, auditState, `name`, candyNum, candyP, inviterMobile, ctime, `level` FROM `user` ");
            if (String.IsNullOrWhiteSpace(query.Mobile))
            {
                QueryUserSql.Append("WHERE id = @UserId;");
                QueryUserParam.Add("UserId", userId, DbType.Int64);
            }
            else
            {
                QueryUserSql.Append("WHERE mobile = @Mobile;");
                QueryUserParam.Add("Mobile", query.Mobile, DbType.String);
            }
            User user = base.dbConnection.QueryFirstOrDefault<User>(QueryUserSql.ToString(), QueryUserParam);
            if (user == null || user.Status != 0) { return rult.SetStatus(ErrorCode.Forbidden, "账号不存在或已被冻结"); }
            userId = user.Id;
            #endregion

            #region 不可兑换矿机验证
            Int32 TaskTotal = 0;
            StringBuilder QueryTaskInfoSql = new StringBuilder();
            if (minningId >= 50 && minningId < 60)
            {
                QueryTaskInfoSql.Append("SELECT COUNT(id) FROM minnings WHERE userId = @UserId AND minningId = @TaskId AND NOW() < endTime AND source != 2;");
                TaskTotal = dbConnection.QueryFirstOrDefault<Int32>(QueryTaskInfoSql.ToString(), new { UserId = userId, TaskId = minningId + 50 });
                TaskTotal += dbConnection.QueryFirstOrDefault<Int32>(QueryTaskInfoSql.ToString(), new { UserId = userId, TaskId = minningId + 60 });
            }
            if (minningId >= 110 && minningId < 120)
            {
                QueryTaskInfoSql.Append("SELECT COUNT(id) FROM minnings WHERE userId = @UserId AND minningId = @TaskId AND NOW() < endTime AND source != 2;");
                TaskTotal = dbConnection.QueryFirstOrDefault<Int32>(QueryTaskInfoSql.ToString(), new { UserId = userId, TaskId = minningId - 10 });
                TaskTotal += dbConnection.QueryFirstOrDefault<Int32>(QueryTaskInfoSql.ToString(), new { UserId = userId, TaskId = minningId - 60 });
            }
            #endregion

            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"Exchange:{userId}", 30);
                if (CacheLock == null) { return rult.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var candyIn = 0;
                try
                {
                    candyIn = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyIn;
                }
                catch (System.Exception ex)
                {
                    LogUtil<SystemService>.Error(ex, "兑换矿机时发生异常");
                    return rult.SetStatus(ErrorCode.Forbidden, "矿机类型错误");
                }
                if (user.CandyNum < candyIn)
                {
                    return rult.SetStatus(ErrorCode.Forbidden, "账户糖果不足");
                }
                //查询现有矿机数量
                var taskCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(*) as count from minnings where userId = {userId} and endTime > now() and minningId = {minningId} AND source != 2;");
                var canTaskCount = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).MaxHave;
                if (TaskTotal >= canTaskCount)
                {
                    return rult.SetStatus(ErrorCode.Forbidden, "你已兑换相同类型矿机");
                }
                if ((taskCount + TaskTotal) >= canTaskCount)
                {
                    return rult.SetStatus(ErrorCode.Forbidden, "该类型矿机超出上限");
                }

                #region 获取用户等级信息
                var SysLevel = AppSetting.Levels.FirstOrDefault(o => o.Level.ToLower().Equals(user.Level.ToLower()));
                if (null == SysLevel) { return rult.SetStatus(ErrorCode.Forbidden, "兑换矿机异常，请联系管理员。"); }
                #endregion
                //=====计算用户兑换矿机获得果皮数量
                decimal GiveUserCandyP = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyP * SysLevel.ExchangeRate;

                //1 新增矿机 2 更新用户糖果 果皮 3 增加果皮流水 增加糖果流水

                Int32 TaskDays = 0;
                String TaskRunTime = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).RunTime.Replace("天", "");
                if (Int32.TryParse(TaskRunTime, out TaskDays))
                {
                    TaskDays = TaskDays + 1;
                }
                var effectiveBiginTime = DateTime.Now.Date.AddDays(1).ToLocalTime().ToString("yyyy-MM-dd");
                var effectiveEndTime = DateTime.Now.Date.AddDays(TaskDays).ToLocalTime().ToString("yyyy-MM-dd");
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        base.dbConnection.Execute($"insert into minnings (userId, minningId, beginTime, endTime, source) values ({userId}, {minningId},'{effectiveBiginTime}' , '{effectiveEndTime}',11)", null, transaction);
                        base.dbConnection.Execute($"update user set candyNum = (candyNum + {-candyIn}),candyP=(candyP+{GiveUserCandyP}) where id = {userId}", null, transaction);
                        base.dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},{GiveUserCandyP},'开启矿机,赠送{GiveUserCandyP}果皮',4,now(),now())", null, transaction);
                        base.dbConnection.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},{-candyIn},'购买矿机消耗" + candyIn + "糖果',4)", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception)
                    {
                        transaction.Rollback();
                        return rult.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();

                //发消息
                try
                {
                    var c = RedisCache.Publish("YoYo_Member_TaskAction", JsonConvert.SerializeObject(new { MemberId = userId, TaskLevel = minningId, Devote = candyIn }));
                    if (c == 0)
                    {
                        LogUtil<YoyoUserSerivce>.Error("YoYo_Member_TaskAction c 消息返送失败");
                    }
                }
                catch { }

                //上级加成
                var inviteUser = base.dbConnection.QueryFirstOrDefault<User>($"SELECT id FROM user WHERE mobile={user.InviterMobile}");
                if (inviteUser != null)
                {
                    #region 写入果皮记录
                    var candyH = (decimal)(Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == minningId).CandyH * 0.05M);
                    String Content = $"直推会员{user.Name}果核{candyH}";

                    StringBuilder InsertSql = new StringBuilder();
                    DynamicParameters InsertParam = new DynamicParameters();
                    InsertSql.Append("INSERT INTO `user_candyp` (`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
                    InsertSql.Append("VALUES (@UserId, @CandyP, @Content, @Source, NOW(), NOW());");

                    InsertParam.Add("UserId", (Int32)inviteUser.Id, DbType.Int32);
                    InsertParam.Add("CandyP", candyH, DbType.Decimal);
                    InsertParam.Add("Content", Content, DbType.String);
                    InsertParam.Add("Source", 3, DbType.Int32);

                    base.dbConnection.Execute(InsertSql.ToString(), InsertParam);
                    #endregion
                }
                rult.Data = true;
                return rult;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("添加矿机失败", ex);
                return rult.SetStatus(ErrorCode.SystemError, "添加矿机失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 延期矿机
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public MyResult<object> PostponeTask(QuerySysTaskModel query)
        {
            MyResult<object> rult = new MyResult<object>();
            if (query.UserId <= 0) { return rult.SetStatus(ErrorCode.InvalidData, "会员编号有误"); }

            DynamicParameters UpdateParam = new DynamicParameters();
            UpdateParam.Add("Days", 1, DbType.Int32);
            UpdateParam.Add("Id", query.Id, DbType.Int64);
            UpdateParam.Add("UserId", query.UserId, DbType.Int32);
            Int32 rows = base.dbConnection.Execute("UPDATE minnings SET endTime = DATE_ADD(endTime, INTERVAL @Days DAY) WHERE userId = @UserId AND id = @Id;", UpdateParam);

            if (rows < 1)
            {
                return rult.SetStatus(ErrorCode.InvalidData, "延期失败");
            }
            return rult;
        }

        /// <summary>
        /// 矿机续期
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public MyResult<object> RenewTask(Int64 taskId, Int64 userId)
        {
            MyResult result = new MyResult();
            if (userId <= 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign token");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"TaskRenew:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 获取会员信息
                StringBuilder QueryUserSql = new StringBuilder();
                DynamicParameters QueryUserParam = new DynamicParameters();
                QueryUserSql.Append("SELECT id, `status`, auditState, `name`, candyNum, candyP, inviterMobile, ctime, `level` FROM `user` WHERE id = @UserId;");
                QueryUserParam.Add("UserId", userId, DbType.Int32);
                User user = base.dbConnection.QueryFirstOrDefault<User>(QueryUserSql.ToString(), QueryUserParam);
                if (user == null || user.Status != 0) { return result.SetStatus(ErrorCode.Forbidden, "账号异常请联系客服"); }
                if (user.AuditState != 2) { return result.SetStatus(ErrorCode.Forbidden, "请先完成实名信息"); }
                #endregion

                //查询现有矿机状态
                var task = base.dbConnection.QueryFirstOrDefault<Minnings>($"select * from minnings where userId = {userId} and id={taskId}");
                if (null == task) { return result.SetStatus(ErrorCode.InvalidData, "非法操作"); }
                if (task.Status.Value == 0) { return result.SetStatus(ErrorCode.InvalidData, "矿机已过期，无法续期"); }
                if (task.EndTime < DateTime.Now) { return result.SetStatus(ErrorCode.InvalidData, "矿机已过期，无法续期"); }

                //不可续期矿机
                //if (!Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).IsRenew) { return result.SetStatus(ErrorCode.InvalidData, "此矿机不可以续期"); }
                //if (task.Source != 1 && task.Source != 11) { return result.SetStatus(ErrorCode.InvalidData, "系统赠送矿机不可以续期"); }
                var candyIn = 0;
                try
                {
                    candyIn = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).CandyIn;
                }
                catch (System.Exception ex)
                {
                    LogUtil<SystemService>.Error(ex, "矿机续期是发生异常");
                    return result.SetStatus(ErrorCode.Forbidden, "矿机类型错误");
                }
                if (user.CandyNum < candyIn)
                {
                    return result.SetStatus(ErrorCode.Forbidden, "账户糖果不足");
                }

                #region 获取用户等级信息
                var SysLevel = AppSetting.Levels.FirstOrDefault(o => o.Level.ToLower().Equals(user.Level.ToLower()));
                if (null == SysLevel) { return result.SetStatus(ErrorCode.Forbidden, "矿机续期异常，请联系管理员。"); }
                #endregion
                //=====计算用户兑换矿机获得果皮数量
                decimal GiveUserCandyP = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId).CandyP * SysLevel.ExchangeRate;

                //1 新增矿机 2 更新用户糖果 果皮 3 增加果皮流水 增加糖果流水
                Int32 Days = 30;
                Tasks TaskInfo = Constants.MinningListSetting.FirstOrDefault(item => item.MinningId == task.MinningId);
                if (!Int32.TryParse(TaskInfo.RunTime.Replace("天", ""), out Days))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "矿机续期失败");
                }
                var effectiveEndTime = task.EndTime.AddDays(Days).ToLocalTime().ToString("yyyy-MM-dd");

                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        base.dbConnection.Execute($"UPDATE `minnings` SET endTime='{effectiveEndTime}',`status`=1 WHERE userId={userId} AND id={task.Id}", null, transaction);
                        base.dbConnection.Execute($"update user set candyNum = (candyNum + {-candyIn}),candyP=(candyP+{GiveUserCandyP}) where id = {userId}", null, transaction);
                        base.dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},{GiveUserCandyP},'矿机续期,赠送{GiveUserCandyP}果皮',4,now(),now())", null, transaction);
                        base.dbConnection.Execute($"insert into `gem_records`(`userId`,`num`,`description`,gemSource) values({userId},{-candyIn},'矿机续期消耗" + candyIn + "糖果',4)", null, transaction);
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        transaction.Rollback();
                        Yoyo.Core.SystemLog.Debug("矿机续期", ex);
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();

                //发消息
                try
                {
                    var c = RedisCache.Publish("YoYo_Member_TaskAction", JsonConvert.SerializeObject(new { MemberId = userId, TaskLevel = task.MinningId, Devote = candyIn, RenewTask = true }));
                    if (c == 0)
                    {
                        LogUtil<YoyoUserSerivce>.Error("YoYo_Member_TaskAction c 消息返送失败");
                    }
                }
                catch (System.Exception)
                {
                }
                result.Data = true;
                return result;
            }
            catch (Exception)
            {
                return result.SetStatus(ErrorCode.SystemError, "矿机续期失败");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 矿机列表
        /// </summary>
        /// <returns></returns>
        public MyResult<List<Tasks>> SysTaskList()
        {
            MyResult<List<Tasks>> rult = new MyResult<List<Tasks>>()
            {
                Data = Constants.MinningListSetting.Where(item => item.MinningId >= 50 || item.MinningId == 16).OrderBy(item => item.MinningId).ToList()
            };
            return rult;
        }

        /// <summary>
        /// 会员矿机列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public MyResult<List<UserTask>> UserTaskList(QuerySysTaskModel query)
        {
            MyResult<List<UserTask>> rult = new MyResult<List<UserTask>>();
            query.PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT t.id AS Id, u.id AS UserId, u.`name` AS UserNick, u.mobile AS UserMobile, t.minningId AS TaskId, t.beginTime, t.endTime, t.createdAt AS CreateTime, t.status, t.source ");
            QuerySql.Append("FROM `user` AS u, minnings AS t WHERE u.id = t.userId ");

            DynamicParameters QueryParam = new DynamicParameters();

            StringBuilder CountSql = new StringBuilder();
            CountSql.Append("SELECT COUNT(u.id) FROM `user` AS u, minnings AS t WHERE u.id = t.userId ");
            if (query.TaskId >= 0)
            {
                CountSql.Append("AND t.minningId = @TaskId ");
                QuerySql.Append("AND t.minningId = @TaskId ");
                QueryParam.Add("TaskId", query.TaskId, DbType.Int32);
            }
            if (!String.IsNullOrWhiteSpace(query.Mobile))
            {
                CountSql.Append("AND u.mobile = @Mobile ");
                QuerySql.Append("AND u.mobile = @Mobile ");
                QueryParam.Add("Mobile", query.Mobile, DbType.String);
            }
            if (!String.IsNullOrWhiteSpace(query.InviterMobile))
            {
                CountSql.Append("AND t.userId IN ( SELECT id FROM `user` WHERE inviterMobile = @InviterMobile) ");
                QuerySql.Append("AND t.userId IN ( SELECT id FROM `user` WHERE inviterMobile = @InviterMobile) ");
                QueryParam.Add("InviterMobile", query.InviterMobile, DbType.String);
            }
            if (query.Status >= 0)
            {
                CountSql.Append("AND t.status = @Status ");
                QuerySql.Append("AND t.status = @Status ");
                QueryParam.Add("Status", query.Status, DbType.Int32);
            }
            if (query.Source >= 0)
            {
                CountSql.Append("AND t.source = @Source ");
                QuerySql.Append("AND t.source = @Source ");
                QueryParam.Add("Source", query.Source, DbType.Int32);
            }

            CountSql.Append(";");
            rult.RecordCount = base.dbConnection.QueryFirstOrDefault<Int32>(CountSql.ToString(), QueryParam);
            rult.PageCount = (rult.RecordCount + query.PageSize - 1) / query.PageSize;

            QuerySql.Append("ORDER BY t.id DESC ");
            QuerySql.Append("LIMIT @PageIndex,@PageSize;");

            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);
            rult.Data = base.dbConnection.Query<UserTask>(QuerySql.ToString(), QueryParam).ToList();

            List<Tasks> SysTask = Constants.MinningListSetting;

            rult.Data = rult.Data.Join(SysTask, d => d.TaskId, s => s.MinningId, (d, s) => new UserTask
            {
                Id = d.Id,
                UserId = d.UserId,
                UserNick = d.UserNick,
                UserMobile = d.UserMobile,
                TaskId = d.TaskId,
                TaskTitle = s.MinningName,
                BeginTime = d.BeginTime,
                EndTime = d.EndTime,
                CreateTime = d.CreateTime,
                Status = d.Status,
                Source = d.Source,
                DailyOutput = s.DayCandyOut,
                TotalOutput = s.CandyOut

            }).ToList();

            return rult;
        }

        #endregion


        public class TaskInfo
        {
            public Int64 UserId { get; set; }
            public Int32 MinningId { get; set; }
            public DateTime BeginTime { get; set; }
        }
    }
}