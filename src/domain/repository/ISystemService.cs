
using domain.configs;
using domain.lfexentitys;
using domain.models;
using domain.models.yoyoDto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace domain.repository
{
    public interface ISystemService
    {
        SysBanner Banner(int id);
        /// <summary>
        /// 广告 轮播
        /// </summary>
        /// <param name="source"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        MyResult<object> Banners(int source, long uid = 0);

        /// <summary>
        /// 推荐排行  【DateTime.Date】
        /// </summary>
        /// <param name="request">月份</param>
        /// <returns></returns>
        Task<List<Ranking>> Recommend(GetRankingRequest request);

        /// <summary>
        /// type==0 本日排行行  1 本月排行榜
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<List<Ranking>> ShareRank(int type = 0);

        /// <summary>
        /// type==0 本日排行行  1 本月排行榜
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<List<Ranking>> Duplicate(int type = 0);

        /// <summary>
        /// 加速任务
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<Object>> QuickenTask(Int64 UserId);

        MyResult<object> TodayTask(int source);
        MyResult<object> Notices(NoticesDto model, int userId);
        MyResult<object> OneNotice();
        MyResult<object> TaskList(int type, int userId);
        Task<MyResult<object>> TasksShop(Int64 UserId,int status);
        Task<MyResult<object>> Exchange(int minningId, int userId);

        MyResult<object> TaskRenew(int taskId, int userId);
        //用户信息
        MyResult<UserReturnDto> UserInfo(int userId);
        MyResult<object> CandyRecord(BaseModel model, int userId);
        MyResult<object> CandyRecordH(BaseModel model, int userId, string mobile);
        MyResult<object> CandyRecordP(BaseModel model, int userId);
        MyResult<object> DoTask(int userId, string mobile);
        //获取任务信息
        Task<List<BaseTask>> GetBaseTask(int userId);
        //客户端下载链接
        MyResult<object> ClientDownloadUrl(string systemName);
        MyResult<object> AppDownloadUrl();

        Task<MyResult<object>> CopyWriting(string type);

        #region 后台管理
        /// <summary>
        /// 添加会员任务
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        MyResult<object> AddUserTask(QuerySysTaskModel query);

        /// <summary>
        /// 延期任务
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        MyResult<object> PostponeTask(QuerySysTaskModel query);

        /// <summary>
        /// 任务续期
        /// </summary>
        /// <param name="taskId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        MyResult<object> RenewTask(Int64 taskId, Int64 userId);


        /// <summary>
        /// 任务列表
        /// </summary>
        /// <returns></returns>
        MyResult<List<Tasks>> SysTaskList();

        /// <summary>
        /// 任务列表
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        MyResult<List<UserTask>> UserTaskList(QuerySysTaskModel query);
        #endregion
    }
}