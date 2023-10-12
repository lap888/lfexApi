using domain.models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace domain.repository
{
    /// <summary>
    /// 分红类
    /// </summary>
    public interface IDividendService
    {
        /// <summary>
        /// 现金分红
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task CashDividend(List<CashDividendModel> models);

        /// <summary>
        /// 糖果分红
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        Task CandyDividend(List<CandyDividendModel> models);
    }
}
