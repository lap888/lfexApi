using application.services;
using domain.configs;
using domain.models.dto;
using infrastructure.action;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webAdmin.Controllers.Base;

namespace webAdmin.Controllers
{
    public class HomeController : WebBaseController
    {

        [AllowAnonymous]
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                string path = HttpContext.Request.Query["from"];
                if (string.IsNullOrEmpty(path))
                {
                    path = CookieUtil.GetCookie(Constants.LAST_LOGIN_PATH);
                }
                if (!string.IsNullOrEmpty(path) && path != "/")
                {
                    return Redirect(System.Web.HttpUtility.UrlDecode(path));
                }
            }
            return View();
        }
        [AllowAnonymous]
        public IActionResult ValidateCode()
        {
            ValidateCode _vierificationCodeServices = new ValidateCode();
            string code = "";
            System.IO.MemoryStream ms = _vierificationCodeServices.Create(out code);
            CookieUtil.AppendCookie(Constants.WEBSITE_VERIFICATION_CODE, DataProtectionUtil.Protect(code));
            return File(ms.ToArray(), @"image/png");
        }

        [Route("Welcome")]
        public ViewResult Welcome()
        {
            return View();
        }
        [AllowAnonymous]
        [Route("Denied")]
        public ViewResult Denied()
        {
            return View();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Action("数据分析", ActionType.DataAsync, 1)]
        public ViewResult DataAsycn()
        {
            return View();
        }
        [Action("角色管理", ActionType.SystemManager, 1)]
        public ViewResult Roles()
        {
            var result = PermissionService.Menus;
            return View(result);
        }
        [Action("操作员管理", ActionType.SystemManager, 2)]
        public ViewResult BackstageUser(AccountViewModel model)
        {
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ViewResult AddMember(string id = "0")
        {
            if (id.Equals("0"))
            {
                ViewBag.title = "添加管理员";
            }
            else
            {
                ViewBag.title = "修改管理员";
            }
            return View();
        }
        // [Action("Banner管理", ActionType.ShiChangManager, 1)]
        // public ViewResult BannerManager()
        // {
        //     return View();
        // }
        public ViewResult BannerAdd_Updata(int? id)
        {
            string title = "添加广告";
            ViewBag.title = title;
            return View();
        }
        // [Action("公告管理", ActionType.ShiChangManager, 2)]
        // public ViewResult Announce()
        // {
        //     return View();
        // }
        // public ViewResult AddAnnounce()
        // {
        //     return View();
        // }

        // [Action("景点管理", ActionType.ShiChangManager, Icon = "glyphicon-plane")]
        // public ViewResult Scenic()
        // {
        //     return View();
        // }
        public ViewResult AddScenic_Update(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.Title = "修改景点";
            }
            else
            {
                ViewBag.Title = "添加景点";
            }
            return View();
        }

        // [Action("用户管理", ActionType.UsersManager, Icon = "glyphicon-user")]
        // public ViewResult UserManager() { return View(); }

        public ViewResult AddUser_Update() { return View(); }

        [Action("流水分析", ActionType.OrderManager, 0)]

        public ViewResult WalletRecords() { return View(); }

        // [Action("店铺管理", ActionType.ShiChangManager, Icon = "glyphicon-shopping-cart")]
        // public ViewResult Shop() { return View(); }

        // [Action("信息类别管理", ActionType.SystemManager)]
        // public ViewResult MessType() { return View(); }

        [Action("Banner管理", ActionType.YoyoService, 0)]
        public ViewResult YoUploadBanner() { return View(); }

        [Action("消息指南", ActionType.YoyoService, 1)]
        public ViewResult YoNoticeManager() { return View(); }
        // [Action("游戏管理", ActionType.YoyoService, 2)]
        // public ViewResult YoGameManager() { return View(); }

        [Action("人工审核", ActionType.YoyoService, 3)]
        public ViewResult YoAdminAuth() { return View(); }

        [Action("设备管理", ActionType.YoyoService, 4)]
        public ViewResult YoDevice() { return View(); }

        [Action("文本编辑器", ActionType.YoyoService, 5)]
        public ViewResult AddAnnounce() { return View(); }

        // [Action("认证刷新", ActionType.YoyoService, 5)]
        // public ViewResult YoOrderGame() { return View(); }

        [Action("订单查询", ActionType.OrderManager, 1)]
        public ViewResult YoTradeOrder() { return View(); }

        [Action("会员查询", ActionType.UserManager, 1)]
        public ViewResult YoUserList() { return View(); }

        [Action("会员任务", ActionType.UserManager, 2)]
        public ViewResult YoUserTask() { return View(); }

        // [Action("任务管理", ActionType.YoBangManager, 1)]
        // public ViewResult YoBangList() { return View(); }

        // [Action("任务记录", ActionType.YoBangManager, 2)]
        // public ViewResult YoBangRecord() { return View(); }

        // [Action("商品维护", ActionType.LuckyDrawManager, 1)]
        // public ViewResult LuckyDrawPrize() { return View(); }

        // [Action("夺宝维护", ActionType.LuckyDrawManager, 2)]
        // public ViewResult LuckyDrawRound() { return View(); }

        [Action("充值订单", ActionType.RechargeManager, 1)]
        public ViewResult RechargeOrders() { return View(); }

    }
}
