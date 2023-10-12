using System.Threading.Tasks;
using domain.models.dto;
using domain.models.yoyoDto;
using domain.lfexentitys;

namespace domain.repository
{
    public interface IYoyoWebService
    {
        //公告列表
        MyResult<object> BannerList(BannerDto model);
        MyResult<object> DelBanner(BannerDto model);
        MyResult<object> AddBanner(BannerDto model);

        //公告列表
        MyResult<object> NoticeList(NoticeDto model);
        MyResult<object> DelNotice(NoticeDto model);
        MyResult<object> AddNotice(NoticeDto model);

        //游戏列表
        MyResult<object> GameList(GameDto model);
        MyResult<object> DelGame(GameDto model);
        MyResult<object> AddGame(GameDto model);
        MyResult<object> GameDetailList(int id);
        MyResult<object> AddGameDetail(GameDto model);

        //人工审核
        MyResult<object> AuthList(AuthDto model);
        MyResult<object> AgreeAuth(AuthDto model);
        MyResult<object> NotAgreeAuth(AuthDto model);

        //设备查询
        MyResult<object> DeviceList(LoginHistoryDto model);
        MyResult<object> DelDevice(LoginHistoryDto model);
        //用户认证查询
        MyResult<object> OrderGameList(OrderGameDto model);
        //刷新订单状态
        Task<MyResult<object>> RefreshOrderGame(OrderGameDto model);




    }
}