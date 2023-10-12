using CSRedis;
using domain.configs;
using domain.models;
using domain.repository;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace application.services
{
    public class DividendService : bases.BaseServiceLfex, IDividendService
    {
        private readonly CSRedisClient RedisCache;
        public DividendService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient) : base(connectionStringList)
        {
            RedisCache = redisClient;
        }
        /// <summary>
        /// 糖果分红
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public Task CandyDividend(List<CandyDividendModel> models)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 现金分红
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public Task CashDividend(List<CashDividendModel> models)
        {
            throw new NotImplementedException();
        }
    }
}
