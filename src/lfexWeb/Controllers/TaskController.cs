using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using domain.configs;
using domain.models;
using domain.repository;
using Microsoft.AspNetCore.Mvc;

namespace webAdmin.Controllers
{
    public class TaskController : Base.WebBaseController
    {
        private readonly ISystemService SysSub;
        public TaskController(ISystemService systemService)
        {
            SysSub = systemService;
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 系统任务列表
        /// </summary>
        /// <returns></returns>
        public MyResult<List<Tasks>> SysTaskList()
        {
            return SysSub.SysTaskList();
        }

        /// <summary>
        /// 会员任务列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public MyResult<List<UserTask>> UserTaskList([FromBody] QuerySysTaskModel query)
        {
            return SysSub.UserTaskList(query);
        }

        // /// <summary>
        // /// 添加任务
        // /// </summary>
        // /// <param name="query"></param>
        // /// <returns></returns>
        // public MyResult<object> AddUserTask([FromBody] QuerySysTaskModel query)
        // {
        //     return SysSub.AddUserTask(query);
        // }

        // /// <summary>
        // /// 会员任务延期一天
        // /// </summary>
        // /// <param name="query"></param>
        // /// <returns></returns>
        // public MyResult<object> PostponeTask([FromBody] QuerySysTaskModel query)
        // {
        //     return SysSub.PostponeTask(query);
        // }

        // /// <summary>
        // /// 续期任务
        // /// </summary>
        // /// <param name="query"></param>
        // /// <returns></returns>
        // public MyResult<object> RenewTask([FromBody] QuerySysTaskModel query)
        // {
        //     return SysSub.RenewTask(query.Id, query.UserId);
        // }

    }
}