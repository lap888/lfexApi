using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webAdmin.Controllers
{
    
    public class DownController : Controller
    {
        [AllowAnonymous]
        public IActionResult Index(String code)
        {
            if (String.IsNullOrWhiteSpace(code))
            {
                return Redirect("https://admin.yybex.cn/lfex/index.html");

            }
            return Redirect($"https://admin.yybex.cn/lfex/index.html?code={code}");
        }
    }

}