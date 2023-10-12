using domain.models.yoyoDto;

namespace domain.repository
{
    public interface IGameService
    {
        //首发游戏
        MyResult<object> FristGame(int type, string platform);
        MyResult<object> GameList(GameListDto model);
        //游戏详情
        MyResult<object> GameDetail(string gameId);
        MyResult<object> GenAuthSdwUrl(int userId, string sdwId);
        MyResult<object> GenAuthSdwUrl2(int userId);
        MyResult<object> QueryPayByChannel();

    }
}