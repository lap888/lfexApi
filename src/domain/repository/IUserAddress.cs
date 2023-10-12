using domain.models.yoyoDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace domain.repository
{
    public interface IUserAddress
    {
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="AddressId"></param>
        /// <returns>布尔型</returns>
        Task<MyResult<object>> DelAddress(int userId, int AddressId);
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="AddressId"></param>
        /// <returns>布尔型</returns>
        Task<MyResult<object>> SetDefault(int userId, int AddressId);
        /// <summary>
        /// 获取地址列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<List<UserAddress>>> AddressList(int userId);
        /// <summary>
        /// 设置地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="req"></param>
        /// <returns>布尔型</returns>
        Task<MyResult<object>> SetAddress(int userId, UserAddress req);
    }
}
