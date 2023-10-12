using application.Utils;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.models.dto;
using domain.models.yoyoDto;
using domain.repository;
using domain.lfexentitys;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace application.services
{
    public class YoyoWebService : bases.BaseServiceLfex, IYoyoWebService
    {
        private readonly CSRedisClient RedisCache;
        private readonly IAlipay AlipaySub;
        public YoyoWebService(IOptionsMonitor<ConnectionStringList> connectionStringList, IAlipay alipay, CSRedisClient redisClient) : base(connectionStringList)
        {
            RedisCache = redisClient;
            AlipaySub = alipay;
        }
        #region Banner
        /// <summary>
        /// 添加banner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> AddBanner(BannerDto model)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(model.ImageUrl))
            {
                return result.SetStatus(ErrorCode.InvalidData, "图片数据非法");
            }
            if (string.IsNullOrEmpty(model.Source.ToString()) || model.Source < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "类型数据非法");
            }
            if (string.IsNullOrEmpty(model.Queue.ToString()) || model.Queue < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "Queue类型数据非法");
            }
            if (model.Source == 2)
            {
                model.Params = model.ContentFwb;
            }
            if (string.IsNullOrEmpty(model.Params))
            {
                return result.SetStatus(ErrorCode.InvalidData, "params不能为空");
            }
            if (string.IsNullOrEmpty(model.CityCode))
            {
                model.CityCode = string.Empty;
            }

            var sql = $"INSERT INTO `sys_banner` ( `queue`, `title`, `imageUrl`, `type`, `source`, `status`, `params`, `cityCode`, `createdAt` ) VALUES({model.Queue},'{model.Title}','{model.ImageUrl}',{model.Types},{model.Source},1,'{model.Params}','{model.CityCode}',now())";
            base.dbConnection.Execute(sql);
            result.Data = true;
            return result;
        }
        /// <summary>
        /// banner list
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> BannerList(BannerDto model)
        {
            MyResult result = new MyResult();
            var sql = $"select * from `sys_banner` where 1=1";
            if (model != null && model.Types != null && model.Types != 0)
            {
                sql += $" and `source`={model.Types}";
            }
            sql += " order by id desc;";
            var banner = base.dbConnection.Query<SysBanner>(sql).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.Data = banner;
            result.PageCount = pageCount;
            result.RecordCount = count;
            return result;
        }

        /// <summary>
        /// del banner
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public MyResult<object> DelBanner(BannerDto model)
        {
            MyResult result = new MyResult();
            var banner = base.First<SysBanner>(predicate => predicate.Id == model.Id);
            base.Delete(banner, true);
            RedisCache.Del("System:Notices");
            result.Data = true;
            return result;
        }
        #endregion


        #region Game
        public MyResult<object> AddGame(GameDto model)
        {
            MyResult result = new MyResult();

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    var sql = $"insert into `game_infos`(`gType`,`gPlatform`,`gTitle`,`isFirstPublish`,`synopsis`,`gtProportionl`,`gtVIP`,`gPinyin`,`gameCategoryId`,`gSize`,`gVersion`,`discount`,`description`,`gH5Url`,`gameSupplierId`,`useGem`,`useGemRate`,`isShow`,`companyShareRatio`,`sdwId`,`gSort`) values('{model.GType}','{model.GPlatform}','{model.GTitle}',{model.IsFirstPublish},'{model.Synopsis}','{model.GtProportionl}','{model.GtVip}','{model.GPinyin}',{model.GameCategoryId},{model.GSize},'{model.GVersion}',{model.Discount},'{model.Description}','{model.GH5url}','{model.GameSupplierId}',{model.UseGem},{model.UseGemRate},{model.IsShow},{model.CompanyShareRatio},{model.SdwId},{model.GSort});select @@IDENTITY";
                    var id = base.dbConnection.ExecuteScalar<int>(sql, null, transaction);
                    //写入Logo
                    var insertPicSql = $"insert into `pictures`(`url`,`imageableType`,`imageableId`,`type`) values('{model.ImageUrl}','GameInfo',{id},'gLogo')";
                    base.dbConnection.Execute(insertPicSql, null, transaction);
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    LogUtil<YoyoWebService>.Error(ex.Message);
                    transaction.Rollback();
                    return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                }
            }
            base.dbConnection.Close();

            return result;
        }

        public MyResult<object> DelGame(GameDto model)
        {
            MyResult result = new MyResult();
            base.dbConnection.Execute($"delete from `game_infos` where id={model.Id}");
            return result;
        }
        public MyResult<object> GameList(GameDto model)
        {
            MyResult result = new MyResult();
            var sql = $"select gi.*,gc.`name` categoryName,p.`url` gameLogoUrl from `game_infos` gi left join `game_categories` gc on gi.`gameCategoryId`=gc.id left join `pictures` p on p.`imageableType`='GameInfo' and p.`imageableId`=gi.id and p.`type`='gLogo' where gi.`isShow`=1 order by gi.id desc";
            var gameList = base.dbConnection.Query(sql).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.Data = gameList;
            result.PageCount = pageCount;
            result.RecordCount = count;
            return result;
        }

        public MyResult<object> GameDetailList(int id)
        {
            MyResult result = new MyResult();
            var gameDetail = base.dbConnection.Query<Pictures>($"select * from `pictures` where `imageableType`='GameInfo' and `type`='gImg' and `imageableId`={id}");
            result.Data = gameDetail;
            return result;
        }

        public MyResult<object> AddGameDetail(GameDto model)
        {
            MyResult result = new MyResult();
            var insertPicSql = $"insert into `pictures`(`url`,`imageableType`,`imageableId`,`type`) values('{model.ImageUrl}','GameInfo',{model.Id},'gImg')";
            base.dbConnection.Execute(insertPicSql);
            return result;
        }
        #endregion

        #region Notice
        public MyResult<object> AddNotice(NoticeDto model)
        {
            MyResult result = new MyResult();
            if (model.Types == 2) { model.Content = model.ContentFwb; } else { model.Content = model.Content; }
            if (string.IsNullOrEmpty(model.Content))
            {
                return result.SetStatus(ErrorCode.InvalidData, "Content不能为空");
            }
            if (string.IsNullOrEmpty(model.Types.ToString()) || model.Types < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "类型数据非法");
            }
            if (model.Types == 2 && string.IsNullOrEmpty(model.ContentFwb))
            {
                return result.SetStatus(ErrorCode.InvalidData, "ContentFwb不能为空");
            }

            base.dbConnection.Execute($"insert into `notice_infos`(`userId`,`title`,`content`,`type`) values({0},'{model.Title}','{model.Content}',{model.Types})");
            if (model.Types == 0) { RedisCache.Del("System:Notices_2_1_10"); RedisCache.Del("System:OneNotice"); }
            result.Data = true;
            return result;
        }

        public MyResult<object> DelNotice(NoticeDto model)
        {
            MyResult result = new MyResult();
            var noticeInfos = base.First<NoticeInfos>(predicate => predicate.Id == model.Id);
            base.Delete(noticeInfos, true);
            RedisCache.Del("System:Notices");
            RedisCache.Del("System:OneNotice");
            result.Data = true;
            return result;
        }

        public MyResult<object> NoticeList(NoticeDto model)
        {
            MyResult result = new MyResult();
            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParam = new DynamicParameters();
            QuerySql.Append("SELECT * FROM `notice_infos` WHERE type !=1 ");
            if (model != null && model.Types != null)
            {
                QuerySql.Append("AND `type`=@Type ");
                QueryParam.Add("Type", model.Types, DbType.Int32);
            }
            QuerySql.Append("ORDER BY id DESC;");
            var notice = base.dbConnection.Query<NoticeInfos>(QuerySql.ToString(), QueryParam).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.Data = notice;
            result.PageCount = pageCount;
            result.RecordCount = count;
            return result;
        }

        public MyResult<object> AuthList(AuthDto model)
        {
            MyResult result = new MyResult();
            var sql = $"select ui.*,u.`auditState`,u.`alipay`,u.`inviterMobile` from `authentication_infos` ui left join user u on ui.userId=u.id where ui.`authType`=1 and u.`auditState`={model.AuthType}";
            if (model.Mobile != null)
            {
                sql += $" and u.mobile='{model.Mobile}'";
            }
            sql+=" order by ui.createdAt desc";
            var auths = base.dbConnection.Query<AuthenticationInfosDto>(sql).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            result.Data = auths;
            result.RecordCount = count;
            result.PageCount = count;
            return result;
        }

        public MyResult<object> AgreeAuth(AuthDto model)
        {
            MyResult result = new MyResult();

            if (model.AuthType == 2)
            {
                //订单是否支付
                // var order = base.dbConnection.QueryFirstOrDefault($"select * from `order_games` where gameAppid=1 and userId={model.UserId} and status=1");
                // if (order == null)
                // {
                //     return result.SetStatus(ErrorCode.InvalidData, "订单未支付 不能实名认证");
                // }

                //写入实名信息 更改实名状态 支付宝号
                base.dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        var rowId = base.dbConnection.Execute($"update user set `auditState`=2,`golds`=(`golds`+50),`level`='LV1',`utime`=now() where id = {model.UserId} and auditState<>2", null, transaction);
                        if (rowId <= 0)
                        {
                            transaction.Rollback();
                            return result.SetStatus(ErrorCode.InvalidData, "重复审核");
                        }
                        //贡献值
                        base.dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({model.UserId},50,'实名认证赠送50贡献值',1,now(),now())", null, transaction);

                        //邀请人
                        var user = base.dbConnection.QueryFirstOrDefault<User>($"select `inviterMobile`,`name` from user where id={model.UserId}", null, transaction);
                        //写入记录
                        var inviterUser = base.dbConnection.QueryFirstOrDefault<User>($"select id,golds from user where mobile='{user.InviterMobile}'", null, transaction);
                        if (inviterUser != null)
                        {
                            base.dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({inviterUser.Id},50,'下级「{user.Name}」实名认证赠送50贡献值',1,now(),now())", null, transaction);
                            //贡献值
                            var gold = (int)inviterUser.Golds + 50;
                            var level = CaculatorGolds(gold);
                            base.dbConnection.Execute($"update user set `golds`={gold},`level`='{level}' where `id`={inviterUser.Id}", null, transaction);
                        }
                        result.Data = new { Golds = 50, Level = "LV1" };
                        transaction.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        LogUtil<SystemService>.Error(ex.Message);
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.SystemError, "系统错误请重试");
                    }
                }
                base.dbConnection.Close();
                try
                {
                    long c = RedisCache.Publish("YoYo_Member_Certified", JsonConvert.SerializeObject(new { MemberId = model.UserId }));
                    if (c == 0)
                    {
                        LogUtil<YoyoUserSerivce>.Info("YoYo_Member_Certified c 消息返送失败");
                    }
                }
                catch (System.Exception)
                {
                    LogUtil<YoyoUserSerivce>.Error("YoYo_Member_Certified 消息异常");
                    return result;
                }
            }
            else
            {
                base.dbConnection.Execute($"update user set `auditState`=3 where id = {model.UserId}");
                base.dbConnection.Execute($"update `authentication_infos` set failReason='{model.Reson}' where id={model.Id}");
            }
            return result;
        }

        private string CaculatorGolds(int golds)
        {
            var level = "LV0";
            if (golds >= 5000)
            {
                level = "LV5";
            }
            else if (golds >= 2000)
            {
                level = "LV4";
            }
            else if (golds >= 1000)
            {
                level = "LV3";
            }
            else if (golds >= 200)
            {
                level = "LV2";
            }
            else if (golds >= 50)
            {
                level = "LV1";
            }
            else
            {
                level = "LV0";
            }
            return level;
        }

        public MyResult<object> NotAgreeAuth(AuthDto model)
        {
            throw new System.NotImplementedException();
        }

        public MyResult<object> DeviceList(LoginHistoryDto model)
        {
            MyResult result = new MyResult();
            var deviceSql = $"select * from `login_history` where 1=1";
            if (model != null && !string.IsNullOrEmpty(model.Mobile))
            {
                deviceSql += $" and `mobile`='{model.Mobile}'";
            }
            deviceSql += $" limit {(model.PageIndex - 1) * model.PageSize},{model.PageSize}";
            var devices = base.dbConnection.Query(deviceSql);//.AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            var count = base.dbConnection.QueryFirstOrDefault<int>($"select count(mobile) from `login_history`");

            result.PageCount = count / model.PageSize;
            result.RecordCount = count;
            result.Data = devices;
            return result;
        }

        public MyResult<object> DelDevice(LoginHistoryDto model)
        {
            MyResult result = new MyResult();
            if (model == null || model.Id < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "Id Error");
            }
            Decimal DeductCandy = 0.50M;
            Boolean IsFail = true;

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("Mobile", model.Mobile, DbType.String);
            User UserInfo = base.dbConnection.QueryFirstOrDefault<User>("SELECT `id`, `status` FROM user WHERE mobile = @Mobile;", QueryParam);

            if (UserInfo == null || UserInfo.Status != 0) { return result.SetStatus(ErrorCode.InvalidData, "用户已被封禁"); }

            QueryParam.Add("Id", model.Id, DbType.Int64);
            QueryParam.Add("UserId", UserInfo.Id, DbType.Int64);
            QueryParam.Add("DeductCandy", DeductCandy, DbType.Decimal);
            QueryParam.Add("CandyDesc", $"设备解绑扣除糖果:{DeductCandy.ToString("0.00")}", DbType.String);
            QueryParam.Add("Source", 13, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    StringBuilder UnbindSql = new StringBuilder();
                    UnbindSql.Append("UPDATE login_history SET systemName = '', uniqueId = '0', unLockCount =( unLockCount + 1 ) WHERE id = @Id;");
                    UnbindSql.Append("UPDATE `user` SET candyNum = candyNum - @DeductCandy WHERE id = @UserId;");
                    UnbindSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) VALUES (@UserId, -@DeductCandy, NOW(), NOW(), @CandyDesc, @Source);");

                    base.dbConnection.Execute(UnbindSql.ToString(), QueryParam, transaction);
                    transaction.Commit();
                    IsFail = false;
                }
                catch (Exception ex)
                {
                    LogUtil<SystemService>.Warn(ex.Message);
                    transaction.Rollback();
                }
            }
            base.dbConnection.Close();
            if (IsFail) { return result.SetStatus(ErrorCode.Forbidden, "设备解绑失败"); }
            result.Data = true;
            return result;
        }

        public MyResult<object> OrderGameList(OrderGameDto model)
        {
            MyResult result = new MyResult();
            var deviceSql = $"select og.id,u.`id` userId,u.mobile,u.name,og.`orderId`,og.`realAmount`,og.`status`,og.`createdAt` from `user` u left join `order_games` og on u.id=og.userId where 1=1";

            var countSql = $"select count(u.id) count from `order_games` og left join user u on og.`userId`=u.`id` where 1=1";
            if (model != null && !string.IsNullOrEmpty(model.Mobile))
            {
                deviceSql += $" and u.`mobile`='{model.Mobile}'";
                countSql += $" and u.`mobile`='{model.Mobile}'";
            }
            if (model != null && !string.IsNullOrEmpty(model.status))
            {
                deviceSql += $" and og.`status`='{model.status}'";
                countSql += $" and og.`status`='{model.status}'";
            }
            deviceSql += $" limit {(model.PageIndex - 1) * model.PageSize},{model.PageSize}";
            var userInfos = base.dbConnection.Query(deviceSql);
            var count = base.dbConnection.QueryFirstOrDefault<int>(countSql);

            result.PageCount = count / model.PageSize;
            result.RecordCount = count;
            result.Data = userInfos;
            return result;
        }

        public async Task<MyResult<object>> RefreshOrderGame(OrderGameDto model)
        {
            MyResult result = new MyResult();
            if (model == null || model.Id < 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "Id Error");
            }
            if (model == null || string.IsNullOrEmpty(model.OrderId))
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单流水号不能为空");
            }
            //查记录是否大于两条
            var orderCount = base.dbConnection.QueryFirstOrDefault<int>($"select count(id) count from `order_games` where userId={model.UserId}");
            if (orderCount > 1)
            {
                base.dbConnection.Execute($"update `order_games` set status=1,updatedAt=NOW() where userId={model.UserId}");
                result.Data = true;
                return result;
            }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery()
            {
                OutTradeNo = model.OrderId
            });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS"))
            {
                return result.SetStatus(ErrorCode.InvalidToken, "支付未完成");
            }
            base.dbConnection.Execute($"update `order_games` set status=1,updatedAt=NOW() where id={model.Id}");
            result.Data = true;
            return result;
        }


        #endregion
    }
}