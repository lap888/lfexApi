using CSRedis;
using domain.configs;
using domain.models;
using domain.repository;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Linq;
using Microsoft.Extensions.Options;

namespace application.services
{
    /// <summary>
    /// 糖果操作类
    /// </summary>
    public class CandyService : bases.BaseServiceLfex, ICandyService
    {
        private readonly CSRedisClient RedisCache;
        public CandyService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient) : base(connectionStringList)
        {
            RedisCache = redisClient;
        }

        /// <summary>
        /// 糖果记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<RecordModel>>> CandyRecord(QueryCandyRecord query)
        {
            MyResult<List<RecordModel>> result = new MyResult<List<RecordModel>>() { Data = new List<RecordModel>() };
            if (query.UserId < 1) { return result; }
            query.PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("UserId", query.UserId, DbType.Int64);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);

            StringBuilder QueryCountSql = new StringBuilder();
            QueryCountSql.Append("SELECT COUNT(id) FROM gem_records WHERE userId = @UserId ");

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT id AS RecordId, num AS OccurAmount, description AS `Desc`, createdAt AS OccurTime ");
            QuerySql.Append("FROM gem_records WHERE userId = @UserId ");

            if (query.Source > 0)
            {
                QueryParam.Add("Source", query.Source);

                QueryCountSql.Append("AND gemSource = @Source ");
                QuerySql.Append("AND gemSource = @Source ");
            }
            QuerySql.Append("ORDER BY id DESC LIMIT @PageIndex,@PageSize;");

            try
            {
                result.RecordCount = await dbConnection.QueryFirstOrDefaultAsync<Int32>(QueryCountSql.ToString(), QueryParam);
                result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
                result.Data = dbConnection.Query<RecordModel>(QuerySql.ToString(), QueryParam).ToList();
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("糖果记录", ex);
            }
            return result;
        }
    }
}
