using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using yoyoApi.Controllers.Base;

namespace yoyoApi.Controllers
{
    /// <summary>
    /// 排行
    /// </summary>
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Produces("application/json")]
    public class RankController : ApiBaseController
    {
    }
}