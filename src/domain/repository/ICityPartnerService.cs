using domain.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace domain.repository
{
    /// <summary>
    /// 城市合伙人
    /// </summary>
    public interface ICityPartnerService
    {
        /// <summary>
        /// 获取城市信息
        /// </summary>
        /// <param name="CityNo"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<CityInfoModel> CityInfo(String CityNo, Int64 UserId);

        /// <summary>
        /// 分红记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<DividendRecord>>> CityRecord(QueryCityRecord query);

        /// <summary>
        /// 设置联系方式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> SetContact(ContactModel model);

    }
}
