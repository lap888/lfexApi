using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.lfexentitys;
using domain.models;
using domain.repository;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yoyo.Core;

namespace application.services
{
    /// <summary>
    /// 城市合伙人
    /// </summary>
    public class CityPartnerService : bases.BaseServiceLfex, ICityPartnerService
    {
        private readonly CSRedisClient RedisCache;
        public CityPartnerService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient) : base(connectionStringList)
        {
            RedisCache = redisClient;
        }

        /// <summary>
        /// 城市信息
        /// </summary>
        /// <param name="CityNo"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<CityInfoModel> CityInfo(string CityNo, long UserId)
        {
            CityInfoModel cityInfo = new CityInfoModel();
            String CityCode = base.dbConnection.QueryFirstOrDefault<String>("SELECT CityCode FROM yoyo_city_master WHERE UserId = @UserId", new { UserId });
            if (!String.IsNullOrWhiteSpace(CityCode))
            {
                CityNo = CityCode;
            }

            #region 缓存
            String CacheKey = $"CityInfo:{CityNo}";
            if (RedisCache.Exists(CacheKey))
            {
                return RedisCache.Get<CityInfoModel>(CacheKey);
            }
            #endregion

            try
            {
                StringBuilder QueryCityInfo = new StringBuilder();
                QueryCityInfo.Append("SELECT ac.CityNo, ct.CityName, ct.UserId, ct.WeChat, ct.Mobile, ac.Candy AS CandyEarnings, ac.Cash AS CashEarnings, ac.People, ");
                QueryCityInfo.Append("CONCAT(DATE_FORMAT(ct.StartTime,'%Y-%m-%d'),'至',DATE_FORMAT(ct.EndTime,'%Y-%m-%d')) AS EffectiveTime ");
                QueryCityInfo.Append("FROM city_earnings AS ac, yoyo_city_master AS ct WHERE ac.CityNo = ct.CityCode AND ac.CityNo = @CityNo;");
                cityInfo = await base.dbConnection.QueryFirstOrDefaultAsync<CityInfoModel>(QueryCityInfo.ToString(), new { CityNo });
                if (cityInfo == null) { return new CityInfoModel(); }

                #region 计算现金收益
                StringBuilder QueryCashEarnings = new StringBuilder();
                QueryCashEarnings.Append("SELECT SUM( Incurred ) FROM user_account_wallet_record WHERE (ModifyType = 11 OR ModifyType BETWEEN 15 AND 18) AND AccountId = ");
                QueryCashEarnings.Append("(SELECT AccountId FROM user_account_wallet WHERE UserId = @UserId);");

                cityInfo.CashEarnings = dbConnection.QueryFirstOrDefault<Decimal?>(QueryCashEarnings.ToString(), new { UserId = cityInfo.UserId }) ?? 0;
                #endregion

                #region 计算糖果收益
                cityInfo.CandyEarnings = dbConnection.QueryFirstOrDefault<Decimal?>("SELECT SUM(num) FROM gem_records WHERE gemSource BETWEEN 50 AND 58 AND userId = @UserId", new { UserId = cityInfo.UserId }) ?? 0;
                #endregion

                #region yo帮分红
                StringBuilder YoBangCandySql = new StringBuilder();
                YoBangCandySql.Append("SELECT SUM(ca.Amount) FROM city_candy_dividend AS ca, yoyo_city_master AS ma WHERE ");
                YoBangCandySql.Append("ca.CityNo = ma.CityCode AND ca.CityNo = @CityNo AND ca.EndDate BETWEEN ma.StartTime AND ma.EndTime AND ca.DividendType = 1 AND ca.State = 2;");
                cityInfo.YoBangCandy = base.dbConnection.QueryFirstOrDefault<Decimal?>(YoBangCandySql.ToString(), new { CityNo }) ?? 0.00M;

                StringBuilder YoBangCashSql = new StringBuilder();
                YoBangCashSql.Append("SELECT SUM(ca.Amount) FROM city_cash_dividend AS ca, yoyo_city_master AS ma WHERE ");
                YoBangCashSql.Append("ca.CityNo = ma.CityCode AND ca.CityNo = @CityNo AND ca.EndDate BETWEEN ma.StartTime AND ma.EndTime AND ca.DividendType = 1 AND ca.State = 2;");
                cityInfo.YoBangCash = base.dbConnection.QueryFirstOrDefault<Decimal?>(YoBangCashSql.ToString(), new { CityNo }) ?? 0.00M;
                #endregion

                #region 视频分红
                StringBuilder VideoCashSql = new StringBuilder();
                VideoCashSql.Append("SELECT SUM(ca.Amount) FROM city_cash_dividend AS ca, yoyo_city_master AS ma WHERE ");
                VideoCashSql.Append("ca.CityNo = ma.CityCode AND ca.CityNo = @CityNo AND ca.EndDate BETWEEN ma.StartTime AND ma.EndTime AND ca.DividendType = 2 AND ca.State = 2;");
                cityInfo.VideoDividend = base.dbConnection.QueryFirstOrDefault<Decimal?>(VideoCashSql.ToString(), new { CityNo }) ?? 0.00M;
                #endregion

                #region 游戏分红
                StringBuilder GameCashSql = new StringBuilder();
                GameCashSql.Append("SELECT SUM(ca.Amount) FROM city_cash_dividend AS ca, yoyo_city_master AS ma WHERE ");
                GameCashSql.Append("ca.CityNo = ma.CityCode AND ca.CityNo = @CityNo AND ca.EndDate BETWEEN ma.StartTime AND ma.EndTime AND ca.DividendType = 3 AND ca.State = 2;");
                cityInfo.GameDividend = base.dbConnection.QueryFirstOrDefault<Decimal?>(GameCashSql.ToString(), new { CityNo }) ?? 0.00M;
                #endregion

                #region 商城分红
                StringBuilder MallCashSql = new StringBuilder();
                MallCashSql.Append("SELECT SUM(ca.Amount) FROM city_cash_dividend AS ca, yoyo_city_master AS ma WHERE ");
                MallCashSql.Append("ca.CityNo = ma.CityCode AND ca.CityNo = @CityNo AND ca.EndDate BETWEEN ma.StartTime AND ma.EndTime AND ca.DividendType = 4 AND ca.State = 2;");
                cityInfo.MallDividend = base.dbConnection.QueryFirstOrDefault<Decimal?>(MallCashSql.ToString(), new { CityNo }) ?? 0.00M;
                #endregion

                #region 任务分红
                cityInfo.TaskCandy = dbConnection.QueryFirstOrDefault<Decimal?>("SELECT SUM(num) FROM gem_records WHERE gemSource = 51 AND userId = @UserId", new { UserId = cityInfo.UserId }) ?? 0;
                #endregion

                #region 交易分红
                cityInfo.TransactionCandy = dbConnection.QueryFirstOrDefault<Decimal?>("SELECT SUM(num) FROM gem_records WHERE gemSource = 50 AND userId = @UserId", new { UserId = cityInfo.UserId }) ?? 0;
                #endregion

                #region 拉新分红
                StringBuilder PushNewSql = new StringBuilder();
                PushNewSql.Append("SELECT SUM( Incurred ) FROM user_account_wallet_record WHERE ModifyType = 11 AND AccountId = ");
                PushNewSql.Append("(SELECT AccountId FROM user_account_wallet WHERE UserId = @UserId);");
                cityInfo.PullNew = base.dbConnection.QueryFirstOrDefault<Decimal?>(PushNewSql.ToString(), new { cityInfo.UserId }) ?? 0.00M;
                #endregion

                #region 城主会员头像
                cityInfo.Avatar = base.dbConnection.QueryFirstOrDefault<String>("SELECT avatarUrl FROM `user` WHERE Id = @UserId;", new { cityInfo.UserId });

                if (String.IsNullOrWhiteSpace(cityInfo.Avatar))
                {
                    cityInfo.Avatar = "images/avatar/default/1.png";
                }
                cityInfo.Avatar = $"https://file.yoyoba.cn/{cityInfo.Avatar}";
                #endregion
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("获取城市信息", ex);
                return cityInfo;
            }


            return cityInfo;
        }

        /// <summary>
        /// 分红记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<DividendRecord>>> CityRecord(QueryCityRecord query)
        {
            MyResult<List<DividendRecord>> result = new MyResult<List<DividendRecord>>()
            {
                Data = new List<DividendRecord>()
            };
            String CityCode = base.dbConnection.QueryFirstOrDefault<String>("SELECT CityCode FROM yoyo_city_master WHERE UserId = @UserId", new { query.UserId });
            if (!String.IsNullOrWhiteSpace(CityCode))
            {
                query.CityNo = CityCode;
            }
            Int64 UserId = await base.dbConnection.QueryFirstOrDefaultAsync<Int64?>("SELECT UserId FROM yoyo_city_master WHERE CityCode = @CityNo;", new { query.CityNo }) ?? 0;
            if (UserId < 1) { return result; }
            StringBuilder CountSql = new StringBuilder();

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("UserId", UserId, DbType.Int64);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);
            if (query.AccountType != 2)
            {
                switch (query.DividendType)
                {
                    case domain.enums.DividendType.YoBang:
                        QueryParam.Add("ModifyType", 15, DbType.Int32);
                        break;
                    case domain.enums.DividendType.Video:
                        QueryParam.Add("ModifyType", 16, DbType.Int32);
                        break;
                    case domain.enums.DividendType.Shandw:
                        QueryParam.Add("ModifyType", 17, DbType.Int32);
                        break;
                    case domain.enums.DividendType.Mall:
                        QueryParam.Add("ModifyType", 18, DbType.Int32);
                        break;
                    case domain.enums.DividendType.PullNew:
                        QueryParam.Add("ModifyType", 11, DbType.Int32);
                        break;
                    default:
                        return result;
                }

                Int64 AccountId = base.dbConnection.QueryFirstOrDefault<Int64?>("SELECT AccountId FROM user_account_wallet WHERE `UserId` = @UserId LIMIT 1;", new { UserId }) ?? 0;
                if (AccountId < 1) { return result; }
                QueryParam.Add("AccountId", AccountId, DbType.Int64);
                CountSql.Append("SELECT COUNT(1) AS `Count` FROM user_account_wallet_record WHERE `AccountId`= @AccountId AND ModifyType = @ModifyType;");

                StringBuilder QueryCashSql = new StringBuilder();
                QueryCashSql.Append("SELECT * FROM user_account_wallet_record ");
                QueryCashSql.Append("WHERE `AccountId`= @AccountId AND ModifyType = @ModifyType ORDER BY RecordId DESC LIMIT @PageIndex,@PageSize;");

                List<UserAccountWalletRecord> records = base.dbConnection.Query<UserAccountWalletRecord>(QueryCashSql.ToString(), QueryParam).ToList();

                foreach (var item in records)
                {
                    result.Data.Add(new DividendRecord()
                    {
                        Title = "城主分红",
                        Amount = item.Incurred,
                        CreateTime = item.ModifyTime,
                        Desc = String.Format(item.ModifyType.GetDescription(), item.ModifyDesc.Split(","))
                    });
                }
            }
            else
            {
                switch (query.DividendType)
                {
                    case domain.enums.DividendType.YoBang:
                        QueryParam.Add("Source", 55, DbType.Int32);
                        break;
                    case domain.enums.DividendType.TaskAddition:
                        QueryParam.Add("Source", 51, DbType.Int32);
                        break;
                    case domain.enums.DividendType.Transaction:
                        QueryParam.Add("Source", 50, DbType.Int32);
                        break;
                    default:
                        return result;
                }

                CountSql.Append("SELECT COUNT(id) FROM gem_records WHERE gemSource = @Source AND userId = @UserId");

                StringBuilder QueryCandySql = new StringBuilder();
                QueryCandySql.Append("SELECT '城主分红' AS Title, num AS Amount, description AS `Desc`, createdAt AS CreateTIme FROM gem_records ");
                QueryCandySql.Append("WHERE gemSource = @Source AND userId = @UserId ORDER BY id DESC LIMIT @PageIndex,@PageSize;");

                result.Data = base.dbConnection.Query<DividendRecord>(QueryCandySql.ToString(), QueryParam).ToList();
            }

            result.RecordCount = base.dbConnection.QueryFirstOrDefault<Int32>(CountSql.ToString(), QueryParam);
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;

            return result;
        }

        /// <summary>
        /// 设置联系方式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> SetContact(ContactModel model)
        {
            MyResult<object> result = new MyResult<object>();
            if (String.IsNullOrWhiteSpace(model.CityNo) || model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "请求参数有误"); }
            String CityCode = await base.dbConnection.QueryFirstOrDefaultAsync<String>("SELECT CityCode FROM yoyo_city_master WHERE UserId = @UserId", new { model.UserId });
            if (String.IsNullOrWhiteSpace(CityCode) || !model.CityNo.Equals(CityCode)) { return result.SetStatus(ErrorCode.InvalidData, "您无权修改本城市,联系方式"); }

            Int32 rows = base.dbConnection.Execute("UPDATE `yoyo_city_master` SET `WeChat` = @WeChat, `Mobile` = @Mobile WHERE `CityCode` = @CityCode", new { CityCode, model.Mobile, model.WeChat });

            if (rows > 0)
            {

                String CacheKey = $"CityInfo:{model.CityNo}";
                RedisCache.Del(CacheKey);
                return result;
            }

            return result.SetStatus(ErrorCode.InvalidData, "修改失败");
        }
    }
}
