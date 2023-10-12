using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using domain.models.yoyoDto;
using domain.repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class UserAddressController : ApiBaseController
    {
        public IUserAddress UserAddress { get; set; }
        public UserAddressController(IUserAddress yoyoAddress)
        {
            UserAddress = yoyoAddress;
        }

        /// <summary>
        /// 获取地址列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<List<UserAddress>>> List()
        {
            return await UserAddress.AddressList(base.TokenModel.Id);
        }

        /// <summary>
        /// 删除地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Del(int id)
        {
            return await UserAddress.DelAddress(base.TokenModel.Id, id);
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<MyResult<object>> Set(int id)
        {
            return await UserAddress.SetDefault(base.TokenModel.Id, id);
        }

        /// <summary>
        /// 编辑地址
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<MyResult<object>> Edit([FromBody]UserAddress req)
        {
            return await UserAddress.SetAddress(base.TokenModel.Id, req);
        }
    }
}