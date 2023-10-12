using domain.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace domain.repository
{
    /// <summary>
    /// 糖果操作类
    /// </summary>
    public interface ICandyService
    {
        /// <summary>
        /// 糖果记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<RecordModel>>> CandyRecord(QueryCandyRecord query);

    }
}
