using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.models.yoyoDto;
using domain.repository;
using domain.lfexentitys;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Text;
using Yoyo.Core;

namespace application.services
{
    public class GameService : bases.BaseServiceLfex, IGameService
    {
        private readonly CSRedisClient RedisCache;
        public GameService(IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient) : base(connectionStringList)
        {
            RedisCache = redisClient;
        }

        public MyResult<object> FristGame(int type, string platform)
        {
            MyResult result = new MyResult();
            var sqlPlatfromFilter = $"(gi.gPlatform='0' or gi.gPlatform='2' or gi.gPlatform='1')";
            if (platform == "ios")
            {
                sqlPlatfromFilter = $"(gi.gPlatform = '0' or gi.gPlatform = '1')";
            }
            else if (platform == "android")
            {
                sqlPlatfromFilter = $"(gi.gPlatform = '0' or gi.gPlatform = '2')";
            }
            var sqlTypeFilter = "";
            //'全部'', ''棋牌'', ''角色'', ''传奇'', ''策略'', ''卡牌'', ''挂机'', ''经营'', ''休闲'', ''女生''
            if (type != 0)
            {
                sqlTypeFilter = $"and gc.`id`={type}";
            }
            var sql = $"select gi.*,gc.`name` categoryName,CONCAT('{Constants.CosUrl}', `url`) AS gameLogoUrl,1288 userNum from `game_infos` gi left join `game_categories` gc on gi.`gameCategoryId`=gc.id left join `pictures` p on p.`imageableType`='GameInfo' and p.`imageableId`=gi.id and p.`type`='gLogo' where {sqlPlatfromFilter} {sqlTypeFilter} and gi.`isFirstPublish`=1 and gi.`isShow`=1 order by gi.id desc limit 1";
            var fristGame = base.dbConnection.QueryFirstOrDefault(sql);
            if (fristGame == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "暂无数据");
            }
            result.Data = fristGame;
            return result;
        }

        public MyResult<object> GameDetail(string gameId)
        {
            MyResult result = new MyResult();

            StringBuilder QuerySql = new StringBuilder();

            DynamicParameters QueryParam = new DynamicParameters();

            QuerySql.Append("SELECT ");
            QuerySql.Append("`id` AS Id, ");
            QuerySql.Append("CONCAT(@CosUrl, `url`) AS `Url`, ");
            QuerySql.Append("`imageableType` AS `ImageableType`, ");
            QuerySql.Append("`imageableId` AS `ImageableId`, ");
            QuerySql.Append("`size` AS `Size`, ");
            QuerySql.Append("`createdAt` AS `createdAt`, ");
            QuerySql.Append("`updatedAt` AS `updatedAt`, ");
            QuerySql.Append("`type` AS `type` ");
            QuerySql.Append("FROM pictures ");
            QuerySql.Append("WHERE imageableType = @ImageType AND imageableId = @GameId AND type = @Type; ");

            QueryParam.Add("CosUrl", Constants.CosUrl, DbType.String);
            QueryParam.Add("ImageType", "GameInfo", DbType.String);
            QueryParam.Add("GameId", gameId, DbType.String);
            QueryParam.Add("Type", "gImg", DbType.String);

            //var sql = $"SELECT * FROM pictures WHERE imageableType='GameInfo' and imageableId='{gameId}' and `type`='gImg'";
            //var gamePicList = base.dbConnection.Query<Pictures>(sql);

            List<Pictures> gamePicList = base.dbConnection.Query<Pictures>(QuerySql.ToString(), QueryParam).ToList();
            result.Data = gamePicList;
            return result;
        }

        public MyResult<object> GameList(GameListDto model)
        {
            MyResult result = new MyResult();
            var sqlPlatfromFilter = $"(gi.gPlatform='0' or gi.gPlatform='2' or gi.gPlatform='1')";
            if (model.Platform == "ios")
            {
                sqlPlatfromFilter = $"(gi.gPlatform = '0' or gi.gPlatform = '1')";
            }
            else if (model.Platform == "android")
            {
                sqlPlatfromFilter = $"(gi.gPlatform = '0' or gi.gPlatform = '2')";
            }
            var sqlTypeFilter = "";
            if (model.Type != 0)
            {
                sqlTypeFilter = $"and gc.`id`={model.Type}";
            }
            var sql = $"select gi.*, gc.`name` categoryName, CONCAT('{Constants.CosUrl}', p.`url`) AS gameLogoUrl from `game_infos` gi left join `game_categories` gc on gi.`gameCategoryId`=gc.id left join `pictures` p on p.`imageableType`='GameInfo' and p.`imageableId`=gi.id and p.`type`='gLogo' where {sqlPlatfromFilter} {sqlTypeFilter} and gi.`isShow`=1 order by gi.gSort";
            var gameList = base.dbConnection.Query(sql).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.Data = gameList;
            result.PageCount = pageCount;
            result.RecordCount = count;
            return result;
        }

        public MyResult<object> GenAuthSdwUrl(int userId, string sdwId)
        {
            MyResult result = new MyResult();
            var AuthUrl = "http://www.shandw.com/auth/";
            //var PreAppPic = "https://d.yoyoba.cn/";
            if (string.IsNullOrEmpty(sdwId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "sdwId 非法");
            }
            var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
            if (userInfo == null)
            {
                userInfo = new User
                {
                    Name = "匿名",
                    AvatarUrl = "images/avatar/default/1.png"
                };
            }
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timeSpan = dto.ToUnixTimeSeconds().ToString().Substring(0, 10);
            var avatarFull = Constants.CosUrl + userInfo.AvatarUrl;
            //var avatarFull = PreAppPic + userInfo.AvatarUrl;
            var sign = SecurityUtil.SdwGenSign(Constants.SdwKey, Constants.SdkChannel, userInfo.Name, userInfo.Id, avatarFull, timeSpan);
            dict.Add("channel", Constants.SdkChannel);
            dict.Add("openid", userId.ToString());
            dict.Add("nick", WebUtility.UrlEncode(userInfo.Name));
            dict.Add("avatar", WebUtility.UrlEncode(avatarFull));
            dict.Add("sex", "0");
            dict.Add("phone", "0");
            dict.Add("time", timeSpan);
            dict.Add("sign", sign);
            dict.Add("gid", sdwId);

            dict.Add("sdw_simple", "1");
            dict.Add("sdw_tt", "1");
            dict.Add("sdw_ld", "0");
            dict.Add("sdw_tl", "1");
            dict.Add("sdw_kf", "1");
            dict.Add("sdw_dl", "1");
            dict.Add("sdw_qd", "1");


            var content = string.Join("&", dict.Where(t => !string.IsNullOrEmpty(t.Value))
                  .Select(t => $"{t.Key}={t.Value}"));
            result.Data = AuthUrl + '?' + content;
            RedisCache.Set($"GamePaly:{userId}", sdwId + "|" + DateTime.Now.ToUnixTime(), 150);
            return result;
        }

        public MyResult<object> GenAuthSdwUrl2(int userId)
        {
            MyResult result = new MyResult();
            var AuthUrl = "http://www.shandw.com/auth/";

            var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select * from user where id={userId}");
            if (userInfo == null)
            {
                userInfo = new User
                {
                    Name = "匿名",
                    AvatarUrl = "images/avatar/default/1.png"
                };
            }
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timeSpan = dto.ToUnixTimeSeconds().ToString().Substring(0, 10);
            var avatarFull = Constants.CosUrl + userInfo.AvatarUrl;
            var sign = SecurityUtil.SdwGenSign(Constants.SdwKey, Constants.SdkChannel, userInfo.Name, userInfo.Id, avatarFull, timeSpan);
            dict.Add("channel", Constants.SdkChannel);
            dict.Add("openid", userId.ToString());
            dict.Add("nick", WebUtility.UrlEncode(userInfo.Name));
            dict.Add("avatar", WebUtility.UrlEncode(avatarFull));
            dict.Add("sex", "0");
            dict.Add("phone", "0");
            dict.Add("time", timeSpan);
            dict.Add("sign", sign);

            dict.Add("sdw_simple", "1");
            dict.Add("sdw_tt", "1");
            dict.Add("sdw_ld", "1");
            dict.Add("sdw_tl", "1");
            dict.Add("sdw_kf", "1");
            dict.Add("sdw_dl", "1");
            dict.Add("sdw_qd", "1");


            var content = string.Join("&", dict.Where(t => !string.IsNullOrEmpty(t.Value))
                  .Select(t => $"{t.Key}={t.Value}"));
            result.Data = AuthUrl + '?' + content;
            return result;
        }

        public MyResult<object> QueryPayByChannel()
        {
            MyResult result = new MyResult();
            var baseUrl = "https://h5gm2.shandw.com/open/channel/queryPayByChannel";
            var account = "yoyoba888";
            var apiKey = Constants.SdwKeyYQ;
            var sign = "";
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now.AddMinutes(-30));
            var sec = dto.ToUnixTimeMilliseconds();
            var channel = Constants.SdkChannel;
            var page = 0;
            var pageSize = 10;
            DateTimeOffset dto1 = new DateTimeOffset(DateTime.Now);
            var stime = dto.ToUnixTimeMilliseconds();

            var etime = dto1.ToUnixTimeMilliseconds();
            var _sign = $"account={account}&channel={channel}&sec={sec}&key={apiKey}";
            sign = SecurityUtil.MD5(_sign);
            // sign=md5(account=[account]&channel=[channel]&sec=[sec]&key=[ApiKey])
            var getUrl = $"{baseUrl}?account={account}&channel={channel}&sec={sec}&sign={sign}&page={page}&pageSize={pageSize}&stime={stime}&etime={etime}";
            var str = HttpUtil.GetString(getUrl).ToLower();
            result.Data = str;
            return result;
        }
    }
}