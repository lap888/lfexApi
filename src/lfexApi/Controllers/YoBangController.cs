// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using application;
// using CSRedis;
// using domain.enums;
// using domain.models;
// using domain.repository;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using yoyoApi.Controllers.Base;

// namespace yoyoApi.Controllers
// {
//     /// <summary>
//     /// Yo帮
//     /// </summary>
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     [Produces("application/json")]
//     public class YoBangController : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly IYoBangService YoBang;
//         public YoBangController(CSRedisClient redisClient, IYoBangService bangService)
//         {
//             RedisCache = redisClient;
//             YoBang = bangService;
//         }

//         /// <summary>
//         /// 分类
//         /// </summary>
//         /// <returns></returns>
//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<MyResult<List<BangTaskCateModel>>> TaskCategories()
//         {
//             return await YoBang.Categories();
//         }

//         /// <summary>
//         /// 列表
//         /// </summary>
//         /// <returns></returns>
//         [HttpPost]
//         [AllowAnonymous]
//         public async Task<MyResult<ListModel<BangTaskInfoModel>>> TaskList([FromBody] BangTaskQueryModel query)
//         {
//             Int64 Tid = 0;
//             if (Int64.TryParse(query.Keyword, out Tid))
//             {
//                 query.TaskId = Tid;
//                 query.Keyword = null;
//             }
//             else
//             {
//                 query.TaskState = domain.enums.TaskState.Normal;
//             }
//             if (query.Sort == TaskSort.Default) { query.Sort = TaskSort.Price; }
//             return await YoBang.TaskList(query);
//         }

//         /// <summary>
//         /// 详情
//         /// </summary>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<BangTaskInfoModel>> TaskDetail([FromBody] BangTaskQueryModel query)
//         {
//             query.UserId = base.TokenModel.Id;
//             return await YoBang.TaskDetail(query);
//         }

//         #region 雇主接口
//         /// <summary>
//         /// 雇主发布任务
//         /// </summary>
//         /// <param name="task"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> PostTask([FromBody] BangTaskInfoModel task)
//         {
//             task.Publisher = base.TokenModel.Id;
//             return await YoBang.PostTask(task);
//         }

//         /// <summary>
//         /// 雇主修改任务
//         /// </summary>
//         /// <param name="task"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> ModifyTask([FromBody] BangTaskInfoModel task)
//         {
//             task.Publisher = base.TokenModel.Id;
//             return await YoBang.ModifyTask(task);
//         }

//         /// <summary>
//         /// 雇主暂停任务
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> PausedTask([FromBody] BangTaskQueryModel query)
//         {
//             query.Publisher = base.TokenModel.Id;
//             return await YoBang.Paused(query);
//         }

//         /// <summary>
//         /// 雇主恢复任务
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> RestoreTask([FromBody] BangTaskQueryModel query)
//         {
//             query.Publisher = base.TokenModel.Id;
//             return await YoBang.Restore(query);
//         }

//         /// <summary>
//         /// 雇主取消任务
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> CancelTask([FromBody] BangTaskQueryModel query)
//         {
//             query.Publisher = base.TokenModel.Id;
//             return await YoBang.Cancel(query);
//         }

//         /// <summary>
//         /// 雇主我的任务
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<ListModel<BangTaskInfoModel>>> MyTask([FromBody] BangTaskQueryModel query)
//         {
//             query.Publisher = base.TokenModel.Id;
//             return await YoBang.MyTaskList(query);
//         }

//         /// <summary>
//         /// 雇主待审核任务记录
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<BangTaskRecordModel>>> TaskRecord([FromBody] BangTaskQueryRecord query)
//         {
//             MyResult<List<BangTaskRecordModel>> result = new MyResult<List<BangTaskRecordModel>>();
//             if (query.TaskId < 1)
//             {
//                 return result.SetStatus(ErrorCode.InvalidData, "请求参数无效");
//             }
//             query.RecordState = domain.enums.TaskRecordState.Submitted;
//             return await YoBang.TaskRecord(query);
//         }

//         /// <summary>
//         /// 雇主任务追加
//         /// </summary>
//         /// <param name="append"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> TaskAppend([FromBody] BangTaskAppendModel append)
//         {
//             return await YoBang.TaskAppend(append);
//         }

//         /// <summary>
//         /// 雇主任务审核
//         /// </summary>
//         /// <param name="audit"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> TaskAudit([FromBody] BangTaskQueryRecord audit)
//         {
//             MyResult<Object> result = new MyResult<object>();
//             audit.UserId = base.TokenModel.Id;
//             if (YoBang.CheckTaskPublisher(audit.UserId, audit.TaskId))
//             {
//                 return await YoBang.TaskAudit(audit);
//             }
//             return result.SetStatus(ErrorCode.InvalidData, "请求参数异常[U]");
//         }

//         #endregion

//         #region 用户接口

//         /// <summary>
//         /// 用户待审核任务记录
//         /// </summary>
//         /// <param name="query"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<List<BangTaskRecordModel>>> MyTaskRecord([FromBody] BangTaskQueryRecord query)
//         {
//             query.UserId = base.TokenModel.Id;
//             return await YoBang.UserTaskRecord(query);
//         }

//         /// <summary>
//         /// 用户任务申诉
//         /// </summary>
//         /// <param name="appeal"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<object>> TaskAppeal(BangTaskAppealModel appeal)
//         {
//             appeal.UserId = base.TokenModel.Id;
//             return await YoBang.TaskAppeal(appeal);
//         }

//         /// <summary>
//         /// 用户任务报名
//         /// </summary>
//         /// <param name="task"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> TaskApply(BangTaskQueryRecord task)
//         {
//             task.UserId = base.TokenModel.Id;
//             return await YoBang.TaskApply(task);
//         }

//         /// <summary>
//         /// 用户取消任务
//         /// </summary>
//         /// <param name="task"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> TaskCancel(BangTaskQueryRecord task)
//         {
//             task.UserId = base.TokenModel.Id;
//             return await YoBang.TaskCancel(task);
//         }

//         /// <summary>
//         /// 用户提交任务
//         /// </summary>
//         /// <param name="task"></param>
//         /// <returns></returns>
//         [HttpPost]
//         public async Task<MyResult<Object>> SubmitTask(BangTaskQueryRecord task)
//         {
//             task.UserId = base.TokenModel.Id;
//             return await YoBang.SubmitTask(task);
//         }

//         #endregion

//     }
// }