using domain.enums;
using domain.models;
using domain.models.dto;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using webAdmin.Controllers.Base;

namespace webAdmin.Controllers
{
    [Route("webapi/[action]")]
    public class WebApiController : WebBaseController
    {
        public IAccountService AccountService { get; set; }
        public IPermissionService PermissionService { get; set; }
        public ISystemService SystemService { get; set; }
        private IMemoryCache MemoryCache;

        public WebApiController(IAccountService accountService, IPermissionService permissionService, ISystemService systemService, IMemoryCache memory)
        {
            AccountService = accountService;
            PermissionService = permissionService;
            SystemService = systemService;
            this.MemoryCache = memory;
        }

        [AllowAnonymous]
        [HttpGet]
        public MyResult<object> AppDownloadUrl()
        {
#if DEBUG
            var res = HttpUtil.GetString("https://adm.52expo.top/webapi/appDownloadUrl");
            return res.GetModel<MyResult<object>>();

#else
            var key = $"C_AppDownloadUrl";
            if (this.MemoryCache.TryGetValue(key, out MyResult<object> result))
            {
                return result;
            }
            result = SystemService.AppDownloadUrl();
            this.MemoryCache.Set(key, result, System.TimeSpan.FromSeconds(5 * 60));
            return result;
#endif
        }
        #region 登录模块
        [AllowAnonymous]
        [HttpPost]
        public MyResult<object> Login([FromBody] BackstageUserAdd model)
        {
            return AccountService.Login(model);
        }

        [HttpGet]
        public MyResult<object> Logout()
        {
            MyResult result = new MyResult();
            return AccountService.LogoutUser();
        }
        #endregion

        #region 添加后台管理员
        [HttpPost]
        public MyResult<object> AddMemberAdd_Update([FromBody] BackstageUserAdd model)
        {
            if (!string.IsNullOrEmpty(model.Id))
            {
                return AccountService.UpdateAccount(model);
            }
            return AccountService.AddAccount(model);
        }
        /// <summary>
        /// 密码修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> MemberPwd_Update([FromBody] BackstageUserAdd model)
        {
            return AccountService.UpdatePwd(model);
        }
        [HttpGet]
        public MyResult<object> GetBackstageUser(string id)
        {
            return AccountService.GetBackstageUser(id);
        }
        /// <summary>
        /// 禁用用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> SetMemberState([FromBody] BackstageUserAdd model)
        {
            if (model.AccountStatus == AccountStatus.Normal)
            {
                model.AccountStatus = AccountStatus.Disabled;
            }
            else
            {
                model.AccountStatus = AccountStatus.Normal;
            }
            return AccountService.UpdateAccount(model);
        }

        #endregion


        #region 后台用户列表
        public MyResult<object> BackstageUser([FromBody] AccountSearchModel model)
        {
            return AccountService.GetBackstageUserList(model);
        }
        #endregion


        #region 角色模块
        [HttpPost]
        public MyResult GetRoles()
        {
            return PermissionService.GetRoles();
        }
        [HttpPost]
        public MyResult<object> SaveRoles([FromBody] RoleModel model)
        {
            return PermissionService.SaveRoles(model);
        }
        [HttpPost]
        public MyResult<object> DeleteRoles([FromBody] RoleModel model)
        {
            return PermissionService.DeleteRoles(model);
        }
        #endregion
    }
}