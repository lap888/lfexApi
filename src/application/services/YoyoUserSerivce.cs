using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using application.services.bases;
using application.Utils;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.models;
using domain.models.admin;
using domain.models.lfexDto;
using domain.models.yoyoDto;
using domain.repository;
using domain.lfexentitys;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Yoyo.Core;

namespace application.services
{
    public class YoyoUserSerivce : BaseServiceLfex, IYoyoUserSerivce
    {
        private readonly String AccountTableName = "user_account_wallet";
        private readonly String RecordTableName = "user_account_wallet_record";
        private readonly String CacheLockKey = "WalletAccount:";
        private readonly CSRedisClient RedisCache;
        private readonly HttpClient Client;
        private readonly IAlipay AlipaySub;
        private readonly IUserWalletAccountService UserWallet;
        private readonly Models.AppSetting AppSetting;
        private readonly IQCloudPlugin QCloudSub;
        public YoyoUserSerivce(IHttpClientFactory factory, IQCloudPlugin cloudPlugin, IUserWalletAccountService userWallet, IAlipay alipay, IOptionsMonitor<ConnectionStringList> connectionStringList, CSRedisClient redisClient, IOptionsMonitor<Models.AppSetting> monitor) : base(connectionStringList)
        {
            RedisCache = redisClient;
            Client = factory.CreateClient("JPushSMS");
            AlipaySub = alipay;
            UserWallet = userWallet;
            QCloudSub = cloudPlugin;
            AppSetting = monitor.CurrentValue;
        }

        /// <summary>
        /// 刷脸认证效验
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> IsFaceAuth(AuthenticationDto model, int UserId)
        {
            MyResult<object> Rult = new MyResult<object>();
            StringBuilder SelectSql = new StringBuilder();
            SelectSql.Append("SELECT `auditState` FROM `user` WHERE `id` = @UserId;");
            User UserInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>(SelectSql.ToString(), new { UserId = UserId });
            if (UserInfo != null && UserInfo.AuditState == 2) { return new MyResult<object>(-1, "您的认证已完成，请务重复操作~"); }

            StringBuilder QueryAlipaySql = new StringBuilder();
            QueryAlipaySql.Append("SELECT `id` FROM `user` WHERE `alipay` = @Alipay AND auditState = 2;");
            User UserAlipay = await base.dbConnection.QueryFirstOrDefaultAsync<User>(QueryAlipaySql.ToString(), new { Alipay = model.Alipay });
            if (UserAlipay != null) { return new MyResult<object>(-1, "支付宝已被使用,请更换其它支付宝号"); }

            StringBuilder QueryPaySql = new StringBuilder();
            QueryPaySql.Append("SELECT `id` FROM `order_games` WHERE `userId` = @UserId AND `status` = 1;");
            OrderGames PayOrder = await base.dbConnection.QueryFirstOrDefaultAsync<OrderGames>(QueryPaySql.ToString(), new { UserId = UserId });
            if (PayOrder == null) { return new MyResult<object>(-1, "请完成支付后，再进行认证~"); }

            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id` FROM `authentication_infos` WHERE `idNum`= @IdNum;");
            AuthenticationInfos AuthInfo = await base.dbConnection.QueryFirstOrDefaultAsync<AuthenticationInfos>(QuerySqlStr.ToString(), new { IdNum = model.IdNum });
            if (AuthInfo != null) { return new MyResult<object>(-1, "身份证号已被使用~"); }

            StringBuilder InitSqlStr = new StringBuilder();
            InitSqlStr.Append("SELECT `id`, `CertifyId`, `CertifyUrl`, `Alipay`, `IsUsed`, `CreateTime` FROM `face_init_record` WHERE `IDCardNum` = @IDCardNum AND `TrueName` = @TrueName ORDER BY id DESC;");
            FaceInitRecord InitRecord = await base.dbConnection.QueryFirstOrDefaultAsync<FaceInitRecord>(InitSqlStr.ToString(), new { TrueName = model.TrueName, IDCardNum = model.IdNum });

            if (InitRecord != null && !string.IsNullOrWhiteSpace(InitRecord.CertifyUrl) && InitRecord.CreateTime > DateTime.Now.AddHours(-24))
            {
                if (!InitRecord.Alipay.Equals(model.Alipay))
                {
                    StringBuilder UpInitSql = new StringBuilder();
                    DynamicParameters UpInitParam = new DynamicParameters();
                    UpInitSql.Append("UPDATE `face_init_record` SET `Alipay` = @Alipay WHERE `Id` = @RecordId");
                    UpInitParam.Add("Alipay", model.Alipay, DbType.String);
                    UpInitParam.Add("RecordId", InitRecord.Id, DbType.Int32);
                    base.dbConnection.Execute(UpInitSql.ToString(), UpInitParam);
                }

                Rult.Data = new domain.models.dto.FaceModel()
                {
                    CertifyId = InitRecord.CertifyId,
                    CertifyUrl = InitRecord.CertifyUrl
                };
                return Rult;
            }

            return null;
        }

        /// <summary>
        /// 扫脸认证【未起用】
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ScanFaceInit(AuthenticationDto model, int userId)
        {
            MyResult<object> Rult = new MyResult<object>();
            StringBuilder SelectSql = new StringBuilder();
            SelectSql.Append("SELECT `auditState` FROM `user` WHERE `id` = @UserId;");
            User UserInfo = base.dbConnection.Query<User>(SelectSql.ToString(), new { UserId = userId }).FirstOrDefault();

            if (UserInfo == null)
            {
                return new MyResult<object>() { Code = -1, Message = "您的操作好像出了点问题~" };
            }
            if (UserInfo.AuditState == 2)
            {
                return new MyResult<object>() { Code = -1, Message = "您的认证已完成，请务重复操作" };
            }

            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id` FROM `authentication_infos` WHERE `userId`= @UserId;");
            AuthenticationInfos AuthInfo = base.dbConnection.Query<AuthenticationInfos>(QuerySqlStr.ToString(), new { Userid = userId }).FirstOrDefault();

            if (AuthInfo == null)
            {
                StringBuilder InsertSqlStr = new StringBuilder();
                InsertSqlStr.Append("INSERT INTO `authentication_infos` ( `userId`, `trueName`, `idNum`, `authType`, `certifyId` ) VALUES( ");
                InsertSqlStr.Append("@UserId, @TrueName, @CertNo, @AuthType, @CertifyId);");
                await base.dbConnection.ExecuteAsync(InsertSqlStr.ToString(), new { UserId = userId, TrueName = model.TrueName, CertNo = model.IdNum, AuthType = model.AuthType, CertifyId = model.CertifyId });
            }
            else
            {
                StringBuilder UpdateSqlStr = new StringBuilder();
                UpdateSqlStr.Append("UPDATE `authentication_infos` SET `trueName` = @TrueName, `idNum` = @CertNo, `authType` = @AuthType, `certifyId` = @CertifyId ");
                UpdateSqlStr.Append(" WHERE `userId` = @UserId");
                await base.dbConnection.ExecuteAsync(UpdateSqlStr.ToString(), new { TrueName = model.TrueName, CertNo = model.IdNum, AuthType = model.AuthType, CertifyId = model.CertifyId, UserId = userId });
            }
            return new MyResult<object>() { Code = 0 };
        }

        /// <summary>
        /// 写入扫脸认证记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task WriteInitRecord(AuthenticationDto model)
        {
            StringBuilder InsertSqlStr = new StringBuilder();
            InsertSqlStr.Append("INSERT INTO `face_init_record` ( `CertifyId`, `CertifyUrl`, `TrueName`, `IDCardNum`, `Alipay`, `IsUsed`, `CreateTime` ) VALUES( ");
            InsertSqlStr.Append("@CertifyId, @CertifyUrl, @TrueName, @IDCardNum, @Alipay, @IsUsed, @CreateTime);");
            await base.dbConnection.ExecuteAsync(InsertSqlStr.ToString(), new { CertifyId = model.CertifyId, CertifyUrl = model.CharacterUrl, TrueName = model.TrueName, IDCardNum = model.IdNum, Alipay = model.Alipay, IsUsed = 0, CreateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") });
        }

        /// <summary>
        /// 扫脸认证记录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<FaceInitRecord> VerfyFaceInit(AuthenticationDto model)
        {
            StringBuilder QuerySqlStr = new StringBuilder();
            QuerySqlStr.Append("SELECT `id`, `CertifyId`, `IDCardNum`, `Alipay` FROM `face_init_record` WHERE `CertifyId` = @CertifyId;");
            FaceInitRecord InitRecord = await base.dbConnection.QueryFirstOrDefaultAsync<FaceInitRecord>(QuerySqlStr.ToString(), new { CertifyId = model.CertifyId, IDCardNum = model.IdNum });
            return InitRecord;
        }

        /// <summary>
        /// 获取会员信息 根据手机号
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> GetUserByMobile(string mobile)
        {
            MyResult<object> result = new MyResult<object>();
            if (string.IsNullOrWhiteSpace(mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
            }
            try
            {
                User UserInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>("SELECT `status`, auditState, `name`, avatarUrl FROM `user` WHERE mobile = @Mobile;", new { Mobile = mobile });

                if (UserInfo == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
                }

                if (UserInfo.Status != 0 || UserInfo.AuditState != 2)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "此会员已被锁定");
                }
                if (!string.IsNullOrWhiteSpace(UserInfo.AvatarUrl))
                {
                    UserInfo.AvatarUrl = "https://file.yoyoba.cn/" + UserInfo.AvatarUrl;
                }
                else
                {
                    UserInfo.AvatarUrl = "https://file.yoyoba.cn/default.png";
                }
                result.Data = UserInfo;
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("获取会员信息 根据手机号", ex);
                return result.SetStatus(ErrorCode.InvalidData, "会员不存在");
            }
        }

        public MyResult<object> Login(YoyoUserDto model)
        {
            MyResult result = new MyResult();
            if (model == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "输入非法");
            }
            if (string.IsNullOrEmpty(model.Mobile))
            {
                return result.SetStatus(ErrorCode.NotFound, "手机号不能为空");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                return result.SetStatus(ErrorCode.NotFound, "密码不能为空");
            }
            if (!ProcessSqlStr(model.Mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            var userSql = $"select u.*,ai.`trueName` from (SELECT user.id,user.auditState,ctime,name,mobile,user.status,todayAvaiableGolds,password,avatarUrl,golds,inviterMobile,user.uuid,cnadyDoAt,level,alipay,freezeCandyNum,candyNum as candyNum,IFNULL(og.status,0) isPay FROM user left join order_games og on user.id=og.userId and og.gameAppid=1 WHERE mobile='{model.Mobile}') u left join `authentication_infos` ai on u.`id`=ai.`userId`";
            var user = base.dbConnection.QueryFirstOrDefault<UserDto>(userSql);
            if (user == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "该账户未注册");
            }
            if (user.Status != 0)
            {
                return result.SetStatus(ErrorCode.Forbidden, "该账户已被封禁,请联系管理员");
            }
            var enPassword = SecurityUtil.MD5(model.Password);
            if (enPassword != user.Password)
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "密码错误");
            }
            model.DeviceName = "";
            //登陆设备绑定
            var deviceInfo = base.dbConnection.QueryFirstOrDefault<LoginHistory>($"SELECT * FROM login_history WHERE uniqueId='{model.UniqueID}' or `mobile`='{model.Mobile}' LIMIT 1");
            if (deviceInfo == null)
            {
                base.dbConnection.Execute("INSERT INTO login_history(userId, mobile, uniqueId, systemName, systemVersion, deviceName, appVersion, ctime, utime) VALUES(@Id,@Mobile,@UniqueID,@SystemName,@SystemVersion,@DeviceName,@Version,NOW(),NOW())", new { user.Id, model.Mobile, model.UniqueID, model.SystemName, model.SystemVersion, model.DeviceName, model.Version });
            }
            else
            {
                if (model.Mobile == deviceInfo.Mobile && (model.UniqueID == deviceInfo.UniqueId || deviceInfo.UniqueId == "0"))
                {
                    //更新登陆时间
                    base.dbConnection.Execute($"UPDATE login_history SET utime=NOW(),uniqueId=@UniqueID WHERE mobile=@Mobile", new { model.UniqueID, model.Mobile });
                }
                else
                {
                    Yoyo.Core.SystemLog.Info(model.GetJson());
                    return result.SetStatus(ErrorCode.InvalidData, "此设备已被其他用户绑定");
                }
            }
            #region 更新地理位置
            if (model.Lat > 0 && model.Lng > 0)
            {
                try
                {
                    UserLocations UserLocation = base.dbConnection.QueryFirstOrDefault<UserLocations>("SELECT * FROM user_locations WHERE UserId = @UserId;", new { UserId = user.Id });
                    DynamicParameters LocationParam = new DynamicParameters();
                    LocationParam.Add("UserId", user.Id, DbType.Int64);
                    LocationParam.Add("Latitude", model.Lat, DbType.Decimal);
                    LocationParam.Add("Longitude", model.Lng, DbType.Decimal);
                    LocationParam.Add("Province", model.Province, DbType.String);
                    LocationParam.Add("ProvinceCode", model.ProvinceCode, DbType.String);
                    LocationParam.Add("City", model.City, DbType.String);
                    LocationParam.Add("CityCode", model.CityCode, DbType.String);
                    LocationParam.Add("Area", model.Area, DbType.String);
                    LocationParam.Add("AreaCode", model.AreaCode, DbType.String);
                    LocationParam.Add("CreateAt", DateTime.Now, DbType.DateTime);
                    LocationParam.Add("UpdateAt", DateTime.Now, DbType.DateTime);
                    if (UserLocation == null)
                    {
                        StringBuilder InsertSql = new StringBuilder();
                        InsertSql.Append("INSERT INTO `user_locations`(`userId`, `latitude`, `longitude`, `province`, `provinceCode`, `city`, `cityCode`, `area`, `areaCode`, `createdAt`, `updatedAt`) ");
                        InsertSql.Append("VALUES (@UserId, @Latitude, @Longitude, @Province, @ProvinceCode, @City, @CityCode, @Area, @AreaCode, @CreateAt, @UpdateAt);");
                        base.dbConnection.Execute(InsertSql.ToString(), LocationParam);
                        //更新 城内人数
                        base.dbConnection.Execute("UPDATE city_earnings SET People = People + 1 WHERE CityNo = @CityNo;", new { CityNo = model.CityCode });
                    }
                    else
                    {
                        LocationParam.Add("Id", UserLocation.Id, DbType.Int64);
                        StringBuilder UpdateSql = new StringBuilder();
                        UpdateSql.Append("UPDATE `user_locations` SET `latitude` = @Latitude, `longitude` = @Longitude, `province` = @Province, ");
                        UpdateSql.Append("`provinceCode` = @ProvinceCode, `city` = @City, `cityCode` = @CityCode, `area` = @Area, `areaCode` = @AreaCode, `updatedAt` = @UpdateAt ");
                        UpdateSql.Append("WHERE `id` = @Id;");
                        base.dbConnection.Execute(UpdateSql.ToString(), LocationParam);
                    }
                }
                catch (Exception ex)
                {
                    Yoyo.Core.SystemLog.Debug(model.GetJson(), ex);
                }
            }
            #endregion
            TokenModel tokenModel = new TokenModel();
            tokenModel.Id = (int)user.Id;
            tokenModel.Mobile = user.Mobile;
            tokenModel.Code = "";
            tokenModel.Source = domain.enums.SourceType.Android;
            //var tokenStr = tokenModel.GetJson();
            //var enToken = DataProtectionUtil.Protect(tokenStr);

            var enToken = Yoyo.Core.AuthToken.SetToken(tokenModel);
            user.Rcode = user.Rcode == null ? "0" : user.Rcode;

            user.AvatarUrl = Constants.CosUrl + user.AvatarUrl ?? "images/avatar/default/1.png";

            result.Data = new
            {
                User = user,
                Token = enToken
            };

            return result;
        }

        public async Task<MyResult<object>> SendVcode(SendVcode model)
        {
            MyResult result = new MyResult();
            try
            {
                if (model == null)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "参数异常");
                }
                if (!DataValidUtil.IsMobile(model.Mobile))
                {
                    return result.SetStatus(ErrorCode.NotFound, "手机号无效");
                }
                if (string.IsNullOrEmpty(model.Type))
                {
                    return result.SetStatus(ErrorCode.NotFound, "类型异常");
                }

                DynamicParameters QueryParam = new DynamicParameters();
                QueryParam.Add("Mobile", model.Mobile, DbType.String);
                User UserInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>("SELECT status FROM user WHERE mobile= @Mobile;", QueryParam);

                switch (model.Type)
                {
                    case "signIn":
                        if (UserInfo != null) { return result.SetStatus(ErrorCode.NotFound, "该账户已经注册"); }
                        break;
                    case "update":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    case "resetPassword":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    case "unbind":
                        if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
                        if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }
                        break;
                    default:
                        break;
                }

                UserVcodes code = base.dbConnection.QueryFirstOrDefault<UserVcodes>($"SELECT createdAt FROM user_vcodes WHERE mobile = @Mobile ORDER BY id DESC LIMIT 1;", QueryParam);
                if (code != null && code.CreatedAt > DateTime.Now.AddMinutes(-10)) { return result.SetStatus(ErrorCode.InvalidData, "验证码有效时长为10分钟，无需重新发送"); }

                MyResult<MsgDto> res = await CommonSendVcode2(model);
                if (res.Data.Msg_Id == null) { return result.SetStatus(ErrorCode.SystemError, res.Data.Error.Message); }
                result.Data = new { msgId = res.Data.Msg_Id };

            }
            catch (System.Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error(ex, $"userId={model.GetJson()}\r\n error={ex.Message}");
                return result.SetStatus(ErrorCode.NotFound, "录入信息非法");
            }
            return result;
        }
        /// <summary>
        /// 发送验证码公共方法
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CommonSendVcode2(SendVcode model)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();
            try
            {
                var MsgId = RedisCache.Get($"VCode:{ model.Mobile}");
                if (!String.IsNullOrWhiteSpace(MsgId))
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "请稍后重试" } } };
                }

                StringContent content = new StringContent(new { mobile = model.Mobile, temp_id = "184448" }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.Client.PostAsync("https://api.sms.jpush.cn/v1/codes", content);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    String res = await response.Content.ReadAsStringAsync();
                    result.Data = res.GetModel<MsgDto>();
                    RedisCache.Set($"VCode:{ model.Mobile}", result.Data.Msg_Id, 600);
                }
                else
                {
                    var res = "{\"error\":{\"code\":50013,\"message\":\"invalid temp_id\"}}";
                    var resModel = res.GetModel<MsgDto>();
                    result.Data = resModel;
                    return result;
                }
            }
            catch (System.Exception)
            {
                var res = "{\"error\":{\"code\":50013,\"message\":\"invalid temp_id\"}}";
                var resModel = res.GetModel<MsgDto>();
                result.Data = resModel;
                return result;
            }
            if (result.Data != null && !string.IsNullOrEmpty(result.Data.Msg_Id))
            {
                #region 写入数据库
                StringBuilder InsertSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                InsertSql.Append("INSERT INTO `user_vcodes`(`mobile`, `msgId`, `createdAt`) VALUES (@Mobile, @MsgId , NOW());");
                Param.Add("Mobile", model.Mobile, DbType.String);
                Param.Add("MsgId", result.Data.Msg_Id, DbType.String);
                await base.dbConnection.ExecuteAsync(InsertSql.ToString(), Param);
                #endregion
            }
            return result;
        }

        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<MsgDto>> CheckVcode(ConfirmVcode model)
        {
            MyResult<MsgDto> result = new MyResult<MsgDto>();
            try
            {
                var MsgId = RedisCache.Get($"VCode:{ model.Mobile}");
                if (String.IsNullOrWhiteSpace(MsgId))
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码非法" } } };
                }
                if (MsgId.Length == 6)
                {
                    if (MsgId.Equals(model.Vcode))
                    {
                        RedisCache.Del($"VCode:{ model.Mobile}");
                        return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = true, Msg_Id = MsgId, Error = new ErrorDto { Code = "0", Message = "验证码正确" } } };
                    }
                    else
                    {
                        return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码错误" } } };
                    }
                }

                if (String.IsNullOrWhiteSpace(MsgId) || MsgId != model.MsgId)
                {
                    return new MyResult<MsgDto> { Data = new MsgDto { Is_Valid = false, Msg_Id = null, Error = new ErrorDto { Code = "-1", Message = "验证码非法" } } };
                }

                StringContent content = new StringContent(new { code = model.Vcode }.GetJson());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await this.Client.PostAsync($"https://api.sms.jpush.cn/v1/codes/{model.MsgId}/valid", content);
                String res = await response.Content.ReadAsStringAsync();

                var resModel = res.GetModel<MsgDto>();

                if (resModel.Is_Valid) { RedisCache.Del($"VCode:{ model.Mobile}"); }

                result.Data = resModel;
            }
            catch (System.Exception)
            {
                var res = "{\"is_valid\":false,\"error\":{\"code\":50026,\"message\":\"wrong msg_id\"}}";
                var resModel = res.GetModel<MsgDto>();
                result.Data = resModel;
                return result;
            }
            return result;
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SignUp(SignUpDto model)
        {
            MyResult result = new MyResult();

            MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.Vcode });

            if (!VerifyRult.Data.Is_Valid)
            {
                return result.SetStatus(ErrorCode.NotFound, "验证码错误");
            }

            if (string.IsNullOrEmpty(model.Mobile))
            {
                return result.SetStatus(ErrorCode.NotFound, "手机号不能为空");
            }
            if (string.IsNullOrEmpty(model.InvitationCode))
            {
                return result.SetStatus(ErrorCode.NotFound, "邀请码不能为空");
            }
            if (string.IsNullOrEmpty(model.NickName))
            {
                return result.SetStatus(ErrorCode.NotFound, "昵称不能为空");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                return result.SetStatus(ErrorCode.NotFound, "密码不能为空");
            }
            //查询验证码
            var code = base.dbConnection.QueryFirstOrDefault<int>($"select count(id) count from user_vcodes where mobile='{model.Mobile}'");
            if (code == 0)
            {
                return result.SetStatus(ErrorCode.NotFound, "手机号未注册");
            }
            //验证码
            var InvitationUser = base.dbConnection.QueryFirstOrDefault<User>($"select id,mobile from `user` where mobile='{model.InvitationCode}' or rcode='{model.InvitationCode}'");
            if (InvitationUser == null)
            {
                return result.SetStatus(ErrorCode.NotFound, "邀请码错误");
            }
            model.InvitationCode = InvitationUser?.Mobile;
            var enPassword = SecurityUtil.MD5(model.Password);
            var enChangePassword = SecurityUtil.MD5("123456");
            //查询用户手机号是否存在
            var mobileIsHave = base.dbConnection.QueryFirstOrDefault<int>($"select id from user where mobile='{model.Mobile}'");
            if (mobileIsHave != 0)
            {
                return result.SetStatus(ErrorCode.HasValued, "手机号已注册");
            }
            int insertUser = 0;
            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    insertUser = base.dbConnection.ExecuteScalar<int>($"INSERT INTO user (mobile, password, passwordSalt, name, inviterMobile, uuid,tradePwd,auditState) VALUES ('{model.Mobile}', '{enPassword}', '', '{model.NickName}','{model.InvitationCode}', '{Guid.NewGuid().ToString("N")}','{enChangePassword}',0);select @@IDENTITY", null, transaction);
                    if (insertUser <= 0)
                    {
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.ErrorSign, "注册失败");
                    }
                    //增加一条用户额外信息记录
                    var rows = base.dbConnection.Execute($"insert into `user_ext`(`userId`) values({insertUser})", null, transaction);
                    if (rows != 1)
                    {
                        transaction.Rollback();
                        return result.SetStatus(ErrorCode.ErrorSign, "注册失败");
                    }
                    //赠送矿机
                    var source = 0;
                    var taskId = 0;
                    var effectiveBiginTime = DateTime.Now.Date.ToLocalTime().ToString("yyyy-MM-dd");
                    var effectiveEndTime = DateTime.Now.Date.AddYears(60).ToLocalTime().ToString("yyyy-MM-dd");
                    await base.dbConnection.ExecuteAsync($"insert into minnings (userId, minningId, beginTime, endTime, source,minningStatus) values ({insertUser}, {taskId}, '{effectiveBiginTime}', '{effectiveEndTime}', {source},0);", null, transaction);
                    //写入币种钱包
                    var coinTypeSql = "select name,type from `coin_type` where status=0 or status=1";
                    var coinTypes = await base.dbConnection.QueryAsync<CoinTypeModel>(coinTypeSql, null, transaction);
                    var flag = false;
                    coinTypes.ToList().ForEach(action =>
                    {
                        rows = base.dbConnection.Execute("INSERT INTO user_account_wallet (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`,`Type`,`CoinType`) VALUES (@UserId, '0', '0', '0', '0', NOW(),@Type,@CoinType)", new { UserId = insertUser, Type = action.Type, CoinType = action.Name }, transaction);
                        if (rows != 1)
                        {
                            transaction.Rollback();
                            flag = true;
                        }
                    });
                    if (flag)
                    {
                        return result.SetStatus(ErrorCode.ErrorSign, "注册失败");
                    }

                    transaction.Commit();
                    result.Data = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug("手机号异常", ex);
                    return result.SetStatus(ErrorCode.ErrorSign, "手机号异常 联系管理员");
                }
                finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
            }

            #region 发送认证信息
            var ParentId = InvitationUser?.Id;
            if (ParentId <= 0 || ParentId == null)
            {
                ParentId = 0;
            }
            try
            {
                var c = RedisCache.Publish("YoYo_Member_Regist", JsonConvert.SerializeObject(new { MemberId = insertUser, ParentId = ParentId }));
                if (c == 0)
                {
                    LogUtil<YoyoUserSerivce>.Error("YoYo_Member_Certified c 消息返送失败");
                }
            }
            catch (System.Exception)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "发消息异常");
            }
            #endregion
            return result;
        }

        public MyResult<User> GetNameByRcode(string mobile)
        {
            MyResult<User> result = new MyResult<User>();
            try
            {
                if (string.IsNullOrEmpty(mobile))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空");
                }
                var user = base.dbConnection.QueryFirstOrDefault<User>($"select name,rcode,avatarUrl from user where mobile =@mobile or rcode=@mobile", new { mobile });
                if (user == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "邀请码有误 请联系推荐人");
                }
                result.Data = user;
            }
            catch (System.Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error($"{ex.Message}");
                return result.SetStatus(ErrorCode.InvalidData, "系统异常 请联系管理员");
            }

            return result;
        }

        public MyResult<object> GetNameByMobile(string mobile)
        {
            MyResult result = new MyResult();
            try
            {
                if (string.IsNullOrEmpty(mobile))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空");
                }
                var user = base.dbConnection.QueryFirstOrDefault<User>($"select name from user where mobile =@mobile or rcode=@mobile", new { mobile });
                if (user == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "邀请码有误 请联系推荐人");
                }
                result.Data = user.Name;
            }
            catch (System.Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error($"{ex.Message}");
                return result.SetStatus(ErrorCode.InvalidData, "系统异常 请联系管理员");
            }

            return result;
        }

        public MyResult<object> WalletAmount(int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.InvalidToken, "sign error");
            }
            var userBalance = base.dbConnection.QueryFirstOrDefault<UserBalance>($"select * from `user_balance` where userId={userId}");
            if (userBalance == null)
            {
                result.Data = new UserBalance
                {
                    BalanceLock = 0,
                    BalanceNormal = 0
                };
                return result;
            }
            result.Data = userBalance;
            return result;
        }

        /// <summary>
        /// 团队信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> TeamInfos(TeamInfosReqDto model, int userId, string mobile)
        {
            MyResult result = new MyResult();
            if (userId < 0) { return result.SetStatus(ErrorCode.InvalidToken, "sign error"); }

            TeamInfosDto myTeamInfo = await base.dbConnection
                .QueryFirstOrDefaultAsync<TeamInfosDto>("SELECT ext.*,CONCAT( @CosUrl, u.`avatarUrl` ) AS `avatarUrl` FROM user_ext AS ext, `user` AS u WHERE u.id = ext.userId AND userId = @UserId;", new { Constants.CosUrl, UserId = userId });
            if (myTeamInfo == null) { myTeamInfo = new TeamInfosDto(); }

            #region 注释代码
            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT u.id, u.`auditState`, u.`mobile`, CONCAT( @CosUrl, u.`avatarUrl` ) AS `avatarUrl`, u.`name`, u.`ctime`, ");
            QuerySql.Append("ue.`authCount`, ue.`bigCandyH`, ue.`teamCandyH`, ue.`teamCount`, ue.`teamStart`, IFNULL(re.id,0) AS Active ");
            QuerySql.Append("FROM `user` AS u LEFT JOIN user_ext ue ON u.id = ue.userId ");
            QuerySql.Append("LEFT JOIN (SELECT id, userId FROM gem_records WHERE TO_DAYS(DATE_ADD(NOW(),INTERVAL -1 DAY)) = TO_DAYS(createdAt) AND gemSource = 1) AS re ON u.id = re.userId ");
            QuerySql.Append("WHERE u.`inviterMobile` = @Mobile ");

            if (model.Type == 0)
            {
                QuerySql.Append("ORDER BY ue.teamCandyH ");
                QuerySql.Append(model.Order);
            }
            else if (model.Type == 1)
            {
                QuerySql.Append("ORDER BY ue.teamCount ");
                QuerySql.Append(model.Order);
            }
            else if (model.Type == 2)
            {
                QuerySql.Append("ORDER BY u.ctime ");
                QuerySql.Append(model.Order);
            }
            else
            {
                QuerySql.Append("ORDER BY u.id ");
                QuerySql.Append(model.Order);
            }
            QuerySql.Append(" LIMIT @PageIndex,@PageSize;");

            result.RecordCount = base.dbConnection.QueryFirstOrDefault<Int32>("SELECT COUNT(id) FROM `user` WHERE `inviterMobile` = @Mobile;", new { Mobile = mobile });
            result.PageCount = (result.RecordCount + model.PageSize - 1) / model.PageSize;

            IEnumerable<TeamInfosDto> teamInfoList = base.dbConnection.Query<TeamInfosDto>(QuerySql.ToString(), new { Constants.CosUrl, Mobile = mobile, PageIndex = (model.PageIndex - 1) * model.PageSize, model.PageSize });

            List<TeamInfosDto> ListData = new List<TeamInfosDto>();

            foreach (var item in teamInfoList)
            {
                TeamInfosDto TeamInfo = item;
                List<Int32> TaskIds = base.dbConnection.Query<Int32>("SELECT `minningId` FROM `minnings` WHERE `userId` = @UserId AND `status` = 1 AND `beginTime` < Now() AND `endTime` > Now() AND source < 3;", new { UserId = item.Id }).ToList();
                Decimal Contributions = 0;

                foreach (var taskId in TaskIds)
                {
                    if (taskId == 0 || taskId == 6 || taskId == 16)
                    {
                        Contributions += 1;
                    }
                    else
                    {
                        Contributions += Constants.TaskListSetting.FirstOrDefault(i => i.MinningId == taskId).TeamH;
                    }
                }

                TeamInfo.Contributions = (Int32)Contributions;
                ListData.Add(TeamInfo);
            }

            #region 团队活跃
            StringBuilder QueryActiveSql = new StringBuilder();
            QueryActiveSql.Append("SELECT COUNT(id) FROM gem_records WHERE gemSource = 1 AND TO_DAYS(DATE_ADD(NOW(),INTERVAL -1 DAY)) = TO_DAYS(createdAt) AND userId IN ");
            QueryActiveSql.Append("(SELECT id FROM `user` WHERE inviterMobile = @Mobile);");
            myTeamInfo.Active = base.dbConnection.QueryFirstOrDefault<Int32?>(QueryActiveSql.ToString(), new { Mobile = mobile }) ?? 0;
            #endregion

            result.Data = new { MyTeamInfo = myTeamInfo, TeamInfoList = teamInfoList };
            return result;
            #endregion

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pushCount"></param>
        /// <param name="teamCandyH"></param>
        /// <param name="littleCandyH"></param>
        /// <returns></returns>
        private int GetStart(int pushCount, int teamCandyH, int littleCandyH)
        {
            // 1. 等级：一星达人 要求：直推实名20人且团队活跃度500点 奖励：一个初级任务且全球交易手续费5%
            // 2. 等级：二星达人 要求：直推实名20人且团队果核2000点且小区果核400点 奖励：一个中级任务且全球交易手续费20%
            // 3. 等级：三星达人 要求：直推实名20人且团队果核8000点且小区果核2000点 奖励：一个高级任务且全球交易手续费15%
            // 4. 等级：四星达人 要求：直推实名20人且团队果核100000点且小区果核25000点 奖励：一个超级任务且全球交易手续费5%
            var teamStart = 0;
            if (pushCount >= 20 && teamCandyH >= 100000 && littleCandyH >= 25000)
            {
                teamStart = 4;
            }
            else if (pushCount >= 20 && teamCandyH >= 8000 && littleCandyH >= 2000)
            {
                teamStart = 3;
            }
            else if (pushCount >= 20 && teamCandyH >= 2000 && littleCandyH >= 400)
            {
                teamStart = 2;
            }
            else if (pushCount >= 20 && teamCandyH >= 500)
            {
                teamStart = 1;
            }
            else
            {
                teamStart = 0;
            }
            return teamStart;

        }

        /// <summary>
        /// 更新团队人数
        /// </summary>
        /// <returns></returns>
        public MyResult<object> UpdateTeamCount()
        {
            MyResult result = new MyResult();
            var userInfos = base.dbConnection.Query<User>($"select id,`inviterMobile` from user where to_days(now())-to_days(ctime)=1");
            userInfos.ToList().ForEach(action =>
            {
                if (action.InviterMobile != "0")
                {
                    var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select id,inviterMobile from user where mobile='{action.InviterMobile}'");
                    if (userInfo != null)
                    {
                        base.dbConnection.Execute($"update user_ext set `teamCount`=(teamCount+1) where userId={userInfo.Id}");
                        //循环走起
                        LoopExetUpdateTeamCount(userInfo.InviterMobile);
                    }
                }

            });
            result.Data = true;
            return result;
        }
        private void LoopExetUpdateTeamCount(string mobile)
        {
            var userInfos = base.dbConnection.QueryFirstOrDefault<User>($"select id,mobile,`inviterMobile` from user where mobile='{mobile}'");
            if (userInfos != null && userInfos.Mobile != "0")
            {
                base.dbConnection.Execute($"update user_ext set `teamCount`=(teamCount+1) where userId={userInfos.Id}");
                //循环走起
                LoopExetUpdateTeamCount(userInfos.InviterMobile);
            }
        }

        /// <summary>
        /// 更新团队活跃度by实名
        /// </summary>
        /// <returns></returns>
        public MyResult<object> UpdateTeamCandyHByAuth()
        {
            MyResult result = new MyResult();
            var userInfos = base.dbConnection.Query<User>($"select u.id,u.`inviterMobile`,u.`auditState`,u.mobile from `authentication_infos` ai left join user u on ai.userId=u.id where u.`auditState`=2 and to_days(now())-to_days(u.ctime)=1");
            userInfos.ToList().ForEach(action =>
            {
                if (action.InviterMobile != "0")
                {
                    var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select id,inviterMobile from user where mobile='{action.InviterMobile}'");
                    if (userInfo != null)
                    {
                        base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH+1) where userId={userInfo.Id}");
                        //循环走起
                        LoopExetUpdateTeamCandyHByAuth(userInfo.InviterMobile);
                    }
                }

            });
            result.Data = true;
            return result;
        }
        private void LoopExetUpdateTeamCandyHByAuth(string mobile)
        {
            var userInfos = base.dbConnection.QueryFirstOrDefault<User>($"select id,mobile,`inviterMobile` from user where mobile='{mobile}'");
            if (userInfos != null && userInfos.Mobile != "0")
            {
                base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH+1) where userId={userInfos.Id}");
                //循环走起
                LoopExetUpdateTeamCandyHByAuth(userInfos.InviterMobile);
            }
        }
        /// <summary>
        /// 根据新增任务添加团队果核
        /// </summary>
        /// <returns></returns>
        public MyResult<object> AddTeamCandyHByTask()
        {
            MyResult result = new MyResult();
            var userInfos = base.dbConnection.Query<TeamCandyHByTaskDto>($"select u.id,u.`inviterMobile`,m.`minningId`,u.mobile from `minnings` m left join user u on m.userId=u.id where m.`source`=1 and to_days(now())-to_days(m.`createdAt`)=1");
            userInfos.ToList().ForEach(action =>
            {
                if (action.Mobile != "0")
                {
                    var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select id,inviterMobile from user where mobile='{action.InviterMobile}'");
                    if (userInfo != null)
                    {
                        var extCandyH = Constants.TaskListSetting.FirstOrDefault(item => item.MinningId == action.MinningId).CandyH;
                        base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH+{extCandyH}) where userId={userInfo.Id}");
                        //循环走起
                        LoopExetUpdateTeamAddCandyHByTask(userInfo.InviterMobile);
                    }
                }

            });
            result.Data = true;
            return result;
        }
        private void LoopExetUpdateTeamAddCandyHByTask(string mobile)
        {
            var userInfos = base.dbConnection.QueryFirstOrDefault<TeamCandyHByTaskDto>($"select u.id,u.`inviterMobile`,m.`minningId`,u.mobile from `minnings` m left join user u on m.userId=u.id where m.`source`=1 and mobile='{mobile}'");
            if (userInfos != null && userInfos.Mobile != "0")
            {
                var extCandyH = Constants.TaskListSetting.FirstOrDefault(item => item.MinningId == userInfos.MinningId).CandyH;
                base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH+{extCandyH}) where userId={userInfos.Id}");
                //循环走起
                LoopExetUpdateTeamAddCandyHByTask(userInfos.InviterMobile);
            }
        }
        /// <summary>
        /// 根据过期任务减去团队果核
        /// </summary>
        /// <returns></returns>
        public MyResult<object> SubTeamCandyHByTask()
        {
            MyResult result = new MyResult();
            var userInfos = base.dbConnection.Query<TeamCandyHByTaskDto>($"select u.id,u.`inviterMobile`,m.`minningId`,u.mobile from `minnings` m left join user u on m.userId=u.id where m.`source`=0 and to_days(now())-to_days(m.`createdAt`)=1");
            userInfos.ToList().ForEach(action =>
            {
                if (action.Mobile != "0")
                {
                    var userInfo = base.dbConnection.QueryFirstOrDefault<User>($"select id,inviterMobile from user where mobile='{action.InviterMobile}'");
                    if (userInfo != null)
                    {
                        var extCandyH = Constants.TaskListSetting.FirstOrDefault(item => item.MinningId == action.MinningId).CandyH;
                        base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH-{extCandyH}) where userId={userInfo.Id}");
                        //循环走起
                        LoopExetUpdateTeamSubCandyHByTask(userInfo.InviterMobile);
                    }
                }

            });
            result.Data = true;
            return result;
        }
        private void LoopExetUpdateTeamSubCandyHByTask(string mobile)
        {
            var userInfos = base.dbConnection.QueryFirstOrDefault<TeamCandyHByTaskDto>($"select u.id,u.`inviterMobile`,m.`minningId`,u.mobile from `minnings` m left join user u on m.userId=u.id where m.`source`=0 and mobile='{mobile}'");
            if (userInfos != null && userInfos.Mobile != "0")
            {
                var extCandyH = Constants.TaskListSetting.FirstOrDefault(item => item.MinningId == userInfos.MinningId).CandyH;
                base.dbConnection.Execute($"update user_ext set `teamCandyH`=(teamCandyH-{extCandyH}) where userId={userInfos.Id}");
                //循环走起
                LoopExetUpdateTeamSubCandyHByTask(userInfos.InviterMobile);
            }
        }

        public async Task<MyResult<object>> GenerateAppUrl(int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"GenerateAppUrl:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 订单信息判断
                var Orders = base.dbConnection.Query<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={userId}").ToList();
                var payOrder = Orders.FirstOrDefault(o => o.Status == 1);
                if (payOrder != null) { return result.SetStatus(ErrorCode.InvalidData, "您已经支付，无需重复支付！"); }
                #endregion
                var orderGame = Orders.FirstOrDefault(o => o.Status == 0);
                var orderNum = Gen.NewGuid();
                Decimal payPrice = 1.50M;
                var res = 0;
                if (orderGame == null)
                {
                    res = base.dbConnection.Execute($"insert into `order_games`(`gameAppid`,`orderId`,`userId`,`uuid`,`realAmount`,`status`,`createdAt`) values(1,'{orderNum}',{userId},0,{payPrice},0,now())");
                }
                else if (orderGame.CreatedAt == null || orderGame.CreatedAt?.AddMinutes(15) < DateTime.Now)
                {
                    StringBuilder UpdateSql = new StringBuilder();
                    DynamicParameters UpdateParam = new DynamicParameters();
                    UpdateSql.Append("UPDATE `order_games` SET `orderId` = @OrderId, `createdAt` = @CreateTime WHERE `id` = @Id");
                    UpdateParam.Add("OrderId", orderNum, DbType.String);
                    UpdateParam.Add("CreateTime", DateTime.Now, DbType.DateTime);
                    UpdateParam.Add("Id", orderGame.Id, DbType.Int32);

                    res = base.dbConnection.Execute(UpdateSql.ToString(), UpdateParam);
                    payPrice = orderGame.RealAmount ?? 1.50M;
                }
                else
                {
                    orderNum = orderGame.OrderId;
                    payPrice = orderGame.RealAmount ?? 1.50M;
                    res = 1;
                }

                if (res != 0)
                {
                    String AppUrl = await AlipaySub.GetSignStr(new Request.ReqAlipayAppSubmit() { OutTradeNo = orderNum, TotalAmount = payPrice.ToString("0.00"), Subject = "哟哟吧实名认证", NotifyUrl = AppSetting.AlipayNotify, TimeOutExpress = "15m", PassbackParams = "AUTH_REAL_NAME" });
                    result.Data = AppUrl;
                }
                else
                {
                    return result.SetStatus(ErrorCode.InvalidData, "生成支付订单失败");
                }
                return result;
            }
            catch (Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error(ex, "退款发生错误");
                return result.SetStatus(ErrorCode.SystemError, "退款发生错误");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        /// <summary>
        /// 实名广告
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> RealNameAd(Int64 UserId)
        {
            MyResult result = new MyResult();
            if (UserId < 0) { return result.SetStatus(ErrorCode.ErrorSign, "Error Sign"); }

            String CacheKey = $"RealAd_Lock:{UserId}";
            if (RedisCache.Exists(CacheKey))
            {
                return result.SetStatus(ErrorCode.InvalidData, "您操作太快了");
            }
            else { RedisCache.Set(CacheKey, UserId, 10); }

            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"RealNameAd:{UserId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                #region 订单信息判断
                var Orders = await base.dbConnection.QueryAsync<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={UserId}");
                var payOrder = Orders.FirstOrDefault(o => o.Status == 1);
                if (payOrder != null) { return result.SetStatus(ErrorCode.InvalidData, "您已观看完成，快去填写认证信息吧"); }
                #endregion
                var orderGame = Orders.FirstOrDefault(o => o.Status == 0);
                var orderNum = Gen.NewGuid();
                Decimal payPrice = 1.50M;
                Int32 TotalWatch = 1;
                Int32 rows = 0;
                if (orderGame == null)
                {
                    rows = base.dbConnection.Execute($"insert into `order_games`(`gameAppid`,`orderId`,`userId`,`uuid`,`realAmount`,`status`,`createdAt`) values(1,'{orderNum}',{UserId},1,{payPrice},0,now());");
                    result.Data = new { TotalWatch };
                }
                else
                {
                    Int32.TryParse(orderGame.Uuid, out TotalWatch);

                    TotalWatch = TotalWatch + 1;

                    StringBuilder UpdateSql = new StringBuilder();
                    DynamicParameters UpdateParam = new DynamicParameters();
                    UpdateSql.Append("UPDATE `order_games` SET `status` = @Status, `orderId` = @OrderId, `uuid` = @Uuid , `updatedAt` = @UpdatedAt WHERE `id` = @Id");
                    UpdateParam.Add("Id", orderGame.Id, DbType.Int32);
                    UpdateParam.Add("OrderId", orderNum, DbType.String);
                    UpdateParam.Add("Uuid", TotalWatch, DbType.String);
                    UpdateParam.Add("UpdatedAt", DateTime.Now, DbType.DateTime);
                    if (TotalWatch >= AppSetting.AuthAdCount)
                    {
                        UpdateParam.Add("Status", 1, DbType.Int32);
                    }
                    else
                    {
                        UpdateParam.Add("Status", 0, DbType.Int32);
                    }
                    rows = base.dbConnection.Execute(UpdateSql.ToString(), UpdateParam);
                    result.Data = new { TotalWatch };
                }
                return result;
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug("实名观看广告异常==>", ex);
                return result.SetStatus(ErrorCode.SystemError, "发生错误[AD]");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }

        }

        /// <summary>
        /// 支付宝退款信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> PayRefund(int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //=====================================使用Redis分布式锁=====================================//
            CSRedisClientLock CacheLock = null;
            try
            {
                //=====================================使用Redis分布式锁=====================================//
                CacheLock = RedisCache.Lock($"PayRefund:{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }
                //=====================================使用Redis分布式锁=====================================//

                var User = base.dbConnection.QueryFirstOrDefault<int>($"SELECT COUNT(id) FROM `user` WHERE auditState=2 AND id={userId}");
                if (User != 0)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "您已完成实名认证，无法退款!");
                }
                var orderGame = base.dbConnection.QueryFirstOrDefault<OrderGames>($"select * from `order_games` where gameAppid=1 and userId={userId} and status=1");
                if (orderGame == null)
                {
                    return result.SetStatus(ErrorCode.NotFound, "没有找到您的认证订单，无法退款！");
                }
                if ((DateTime.Now - orderGame.CreatedAt.Value).TotalDays > 7)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "订单已超过7天，无法退款！");
                }
                if ((DateTime.Now - orderGame.CreatedAt.Value).TotalMinutes < 5)
                {
                    return result.SetStatus(ErrorCode.InvalidData, "支付成功后，5分钟内不可退款！");
                }
                if (orderGame.CreatedAt < DateTime.Parse("2020-03-16"))
                {
                    return result.SetStatus(ErrorCode.InvalidData, "公测期订单，请求联系客服~");
                }
                var aliResult = await AlipaySub.Execute(new Request.ReqAlipayTradeRefund
                {
                    OutTradeNo = orderGame.OrderId,
                    RefundReason = "实名认证失败退款",
                    RefundAmount = orderGame.RealAmount.Value
                });
                if (aliResult.IsError)
                {
                    return result.SetStatus(ErrorCode.SystemError, aliResult.ErrMsg);
                }
                StringBuilder UpdateSql = new StringBuilder();
                DynamicParameters Param = new DynamicParameters();
                UpdateSql.Append("UPDATE `order_games` SET `status` = 5,`updatedAt`=NOW() WHERE `orderId` = @OrderId");
                Param.Add("OrderId", orderGame.OrderId, DbType.String);
                base.dbConnection.Execute(UpdateSql.ToString(), Param);

                result.Data = true;
                result.Message = "退款成功";
                return result;
            }
            catch (Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error(ex, "退款发生错误");
                return result.SetStatus(ErrorCode.SystemError, "退款发生错误");
            }
            finally
            {
                //=====================================使用Redis分布式锁=====================================//
                if (null != CacheLock) { CacheLock.Unlock(); }
                //=====================================使用Redis分布式锁=====================================//
            }
        }

        public async Task<String> AliNotify(String TradeNo)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(TradeNo)) { return "fail"; }
                AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery { OutTradeNo = TradeNo });
                if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS")) { return "fail"; }
                base.dbConnection.Execute($"update `order_games` set `status`=1,updatedAt=NOW() where orderId='{TradeNo}' limit 1");
                return "success";
            }
            catch
            {
                return "fail";
            }

        }
        public async Task<MyResult<object>> HavePayOrder(int userId)
        {
            MyResult result = new MyResult();
            var count = await base.dbConnection.QueryFirstOrDefaultAsync<int>($"select count(id) count from `order_games` where userId={userId} and gameAppid=1 and status=1 limit 1");
            if (count == 0)
            {
                return result.SetStatus(ErrorCode.NotFound, "未支付");
            }
            result.Data = true;
            return result;
        }

        public async Task<MyResult<object>> PayFlag(int userId, string outTradeNo)
        {
            MyResult result = new MyResult();
            if (!ProcessSqlStr(outTradeNo))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            AlipayResult<Response.RspAlipayTradeQuery> PayRult = await AlipaySub.Execute(new Request.ReqAlipayTradeQuery()
            {
                OutTradeNo = outTradeNo
            });
            if (PayRult.IsError || !PayRult.Result.TradeStatus.Equals("TRADE_SUCCESS"))
            {
                return result.SetStatus(ErrorCode.InvalidToken, "支付未完成");
            }

            base.dbConnection.Execute($"update `order_games` set `status`=1,updatedAt=NOW() where orderId='{outTradeNo}' and userId={userId}");
            result.Data = true;
            return result;
        }

        public MyResult<object> Authentication(AuthenticationDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            if (reg.IsMatch(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝号不能包含中文");
            }
            if (!ProcessSqlStr(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝号禁止使用特殊符号");
            }
            //订单是否支付
            var order = base.dbConnection.QueryFirstOrDefault($"select * from `order_games` where gameAppid=1 and userId={userId} and status=1");
            if (order == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "订单未支付 不能实名认证");
            }

            //身份证号检验
            var authInfo = base.dbConnection.QueryFirstOrDefault<int>($"select id from `authentication_infos` where `idNum`='{model.IdNum}'");
            if (authInfo != 0)
            {
                return result.SetStatus(ErrorCode.InvalidData, "身份证号已存在");
            }
            //写入实名信息 更改实名状态 支付宝号

            //邀请人===从事务中取出，减少锁时间
            var user = base.dbConnection.QueryFirstOrDefault<User>($"select `inviterMobile`,`name` from user where id={userId}");
            if (user == null) { return result.SetStatus(ErrorCode.InvalidData, "账号不存在"); }
            var inviterUser = base.dbConnection.QueryFirstOrDefault<User>($"select id,golds from user where mobile='{user.InviterMobile}'");
            if (inviterUser == null) { return result.SetStatus(ErrorCode.InvalidData, "信息有误，请联系管理员"); }
            //贡献值
            var InviterDevote = base.dbConnection.ExecuteScalar<decimal>($"SELECT IFNULL(SUM(Devote),0) FROM yoyo_member_devote WHERE UserId={inviterUser.Id};");
            var gold = (int)inviterUser.Golds + 50;
            var level = CaculatorGolds(gold + InviterDevote);
            //计算任务时效
            var source = 0;
            var taskId = 20;
            var effectiveBiginTime = DateTime.Now.Date.ToLocalTime().ToString("yyyy-MM-dd");
            var effectiveEndTime = DateTime.Now.Date.AddDays(60).ToLocalTime().ToString("yyyy-MM-dd");
            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    StringBuilder Sql = new StringBuilder();
                    //修改实名状态
                    Sql.AppendLine($"update user set `auditState`=2,`alipay`='{model.Alipay}',`candyP`=(`candyP`+2),`golds`=(`golds`+50),`level`='lv1',`alipayUid`='' where id = {userId};");
                    //实名赠送果皮
                    Sql.AppendLine($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},2,'实名认证赠送2果皮',4,now(),now());");
                    //修改实名记录
                    Sql.AppendLine($"insert into `authentication_infos`(`userId`,`trueName`,`idNum`,`authType`, `certifyId`) values({userId},'{model.TrueName}','{model.IdNum}',{model.AuthType},'{model.CertifyId}');");
                    //给予上级果皮
                    Sql.AppendLine($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({inviterUser.Id},2,'下级「{user.Name}」实名认证赠送2果皮',4,now(),now());");
                    //修改上级等级
                    Sql.AppendLine($"update user set `candyP`=(`candyP`+2),`golds`={gold},`level`='{level}' where `id`={inviterUser.Id};");
                    //赠送任务
                    Sql.AppendLine($"insert into minnings (userId, minningId, beginTime, endTime, source) values ({userId}, {taskId}, '{effectiveBiginTime}', '{effectiveEndTime}', {source});");

                    var SqlString = Sql.ToString();
                    base.dbConnection.Execute(SqlString, null, transaction);

                    result.Data = new { Golds = 50, Level = "lv1", CandyP = 2 };
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
                StringBuilder UpdateSqlStr = new StringBuilder();
                UpdateSqlStr.Append("UPDATE `face_init_record` SET `IsUsed` = 1 WHERE `CertifyId` = @CertifyId AND `IDCardNum` = @IDCardNum;");
                base.dbConnection.Execute(UpdateSqlStr.ToString(), new { CertifyId = model.CertifyId, IDCardNum = model.IdNum });

                long c = RedisCache.Publish("YoYo_Member_Certified", JsonConvert.SerializeObject(new { MemberId = userId }));
                if (c == 0)
                {
                    LogUtil<YoyoUserSerivce>.Info("YoYo_Member_Certified c 消息发送失败");
                }
            }
            catch (System.Exception)
            {
                LogUtil<YoyoUserSerivce>.Error("YoYo_Member_Certified 消息异常");
                return result;
            }
            return result;
        }

        private string CaculatorGolds(decimal golds)
        {
            string UserLevel = String.Empty;
            foreach (var item in AppSetting.Levels.OrderBy(o => o.Claim))
            {
                if (golds >= item.Claim) { UserLevel = item.Level; }
            }
            return UserLevel;

            #region 注释原代码
            //var level = "lv0";
            //if (golds >= 5000)
            //{
            //    level = "lv5";
            //}
            //else if (golds >= 2000)
            //{
            //    level = "lv4";
            //}
            //else if (golds >= 200)
            //{
            //    level = "lv3";
            //}
            //else if (golds >= 100)
            //{
            //    level = "lv2";
            //}
            //else if (golds >= 50)
            //{
            //    level = "lv1";
            //}
            //else
            //{
            //    level = "lv0";
            //}
            //return level;
            #endregion

        }

        public MyResult<object> ModifyUserPic(YoyoUserDto model, int userId)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(model.UserPic))
            {
                return result.SetStatus(ErrorCode.InvalidData, "头像不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (!ProcessSqlStr(model.UserPic))
            {
                return result.SetStatus(ErrorCode.InvalidData, "头像路径非法");
            }
            //更新头像
            base.dbConnection.Execute($"update user set `avatarUrl`='{model.UserPic}' where id={userId}");
            result.Data = Constants.CosUrl + model.UserPic;
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Str"></param>
        /// <returns></returns>
        private static bool ProcessSqlStr(string Str)
        {
            string SqlStr;
            SqlStr = " |,|=|'|and|exec|insert|select|delete|update|count|*|chr|mid|master|truncate|char|declare";
            Str = Str.ToLower();
            bool ReturnValue = true;
            if (String.IsNullOrWhiteSpace(Str)) { return ReturnValue; }
            try
            {
                if (Str != "")
                {
                    string[] anySqlStr = SqlStr.Split('|');
                    foreach (string ss in anySqlStr)
                    {
                        if (Str.IndexOf(ss) >= 0)
                        {
                            ReturnValue = false;
                        }
                    }
                }
            }
            catch
            {
                ReturnValue = false;
            }
            return ReturnValue;
        }
        public MyResult<object> ModifyUserName(string name, int userId)
        {
            MyResult result = new MyResult();

            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "昵称禁止使用特殊符号");
            }
            if (string.IsNullOrEmpty(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "昵称不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //更新昵称
            base.dbConnection.Execute($"update user set `name`='{name}' where id={userId}");
            result.Data = name;
            return result;
        }
        public async Task<MyResult<object>> ModifyUserInviterCode(string name, int userId)
        {
            MyResult result = new MyResult();
            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            if (reg.IsMatch(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码不能包含中文");
            }
            if (!ProcessSqlStr(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码禁止使用特殊符号");
            }
            if (name.Length > 8)
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码最大长度为8位");
            }
            if (string.IsNullOrEmpty(name))
            {
                return result.SetStatus(ErrorCode.InvalidData, "邀请码不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            var keyFlag = MemoryCacheUtil.Get($"ModifyUserInviterCode{userId}");
            try
            {
                if (keyFlag == null)
                {
                    MemoryCacheUtil.Set($"ModifyUserInviterCode{userId}", 300, 1);
                    //查邀请码是否存在
                    var inviter = base.dbConnection.QueryFirstOrDefault<int>($"select count(1) count from user where rcode='{name}' or mobile='{name}'");
                    if (inviter != 0)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "邀请码已存在");
                    }
                    //移除缓存
                    var oldInviter = base.dbConnection.QueryFirstOrDefault<string>($"select rcode from user where id={userId}");
                    if (!string.IsNullOrWhiteSpace(oldInviter)) { RedisCache.Del($"UserRcode:{oldInviter}"); }
                    var res1 = await ChangeWalletAmount(userId, "LF", -0.5M, LfexCoinnModifyType.Lf_Modify_Code, false, 0.5.ToString());
                    if (res1.Code != 200)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, res1.Message);
                    }
                    //更新昵称
                    base.dbConnection.Execute($"update user set `rcode`='{name}' where id={userId}");
                    result.Data = name;
                }
                else
                {
                    return result.SetStatus(ErrorCode.InvalidData, "更换太快了，请稍后重试...");
                }
            }
            catch (System.Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error($"ModifyUserInviterCode_{userId}_{ex.Message}");
                return result.SetStatus(ErrorCode.SystemError, "系统错误 请稍后再试");
            }
            return result;
        }
        /// <summary>
        /// Coin钱包账户余额变更 common
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="Amount"></param>
        /// <param name="useFrozen">使用冻结金额，账户金额增加时，此参数无效</param>
        /// <param name="modifyType">账户变更类型</param>
        /// <param name="Desc">描述</param>
        /// <returns></returns>
        public async Task<MyResult<object>> ChangeWalletAmount(long userId, string coinType, decimal Amount, LfexCoinnModifyType modifyType, bool useFrozen, params string[] Desc)
        {
            MyResult result = new MyResult { Data = false };
            if (Amount == 0) { return new MyResult { Data = true }; }   //账户无变动，直接返回成功
            if (Amount > 0 && useFrozen) { useFrozen = false; } //账户增加时，无法使用冻结金额
            CSRedisClientLock CacheLock = null;
            UserAccountWallet UserAccount;
            Int64 AccountId;
            String Field = String.Empty, EditSQl = String.Empty, RecordSql = String.Empty, PostChangeSql = String.Empty;
            try
            {
                CacheLock = RedisCache.Lock($"{CacheLockKey}Init_{userId}", 30);
                if (CacheLock == null) { return result.SetStatus(ErrorCode.InvalidData, "请稍后操作"); }

                #region 验证账户信息
                String SelectSql = $"SELECT * FROM `{AccountTableName}` WHERE `UserId` = {userId} AND `CoinType`='{coinType}' LIMIT 1";
                UserAccount = await base.dbConnection.QueryFirstOrDefaultAsync<UserAccountWallet>(SelectSql);
                if (UserAccount == null) { return result.SetStatus(ErrorCode.InvalidData, "账户不存在"); }
                if (Amount < 0)
                {
                    if (useFrozen)
                    {
                        if (UserAccount.Frozen < Math.Abs(Amount) || UserAccount.Balance < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[F]"); }
                    }
                    else
                    {
                        if ((UserAccount.Balance - UserAccount.Frozen) < Math.Abs(Amount)) { return result.SetStatus(ErrorCode.InvalidData, "账户余额不足[B]"); }
                    }
                }
                #endregion

                AccountId = UserAccount.AccountId;
                Field = Amount > 0 ? "Revenue" : "Expenses";

                EditSQl = $"UPDATE `{AccountTableName}` SET `Balance`=`Balance`+{Amount},{(useFrozen ? $"`Frozen`=`Frozen`+{Amount}," : "")}`{Field}`=`{Field}`+{Math.Abs(Amount)},`ModifyTime`=NOW() WHERE `AccountId`={AccountId} {(useFrozen ? $"AND (`Frozen`+{Amount})>=0;" : $"AND(`Balance`-`Frozen`+{Amount}) >= 0;")}";

                PostChangeSql = $"IFNULL((SELECT `PostChange` FROM `{RecordTableName}` WHERE `AccountId`={AccountId} ORDER BY `RecordId` DESC LIMIT 1),0)";
                StringBuilder TempRecordSql = new StringBuilder($"INSERT INTO `{RecordTableName}` ");
                TempRecordSql.Append("( `AccountId`, `PreChange`, `Incurred`, `PostChange`, `ModifyType`, `ModifyDesc`, `ModifyTime` ) ");
                TempRecordSql.Append($"SELECT {AccountId} AS `AccountId`, ");
                TempRecordSql.Append($"{PostChangeSql} AS `PreChange`, ");
                TempRecordSql.Append($"{Amount} AS `Incurred`, ");
                TempRecordSql.Append($"{PostChangeSql}+{Amount} AS `PostChange`, ");
                TempRecordSql.Append($"{(int)modifyType} AS `ModifyType`, ");
                TempRecordSql.Append($"'{String.Join(',', Desc)}' AS `ModifyDesc`, ");
                TempRecordSql.Append($"'{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' AS`ModifyTime`");
                RecordSql = TempRecordSql.ToString();

                #region 修改账务
                if (base.dbConnection.State == ConnectionState.Closed) { base.dbConnection.Open(); }
                using (IDbTransaction Tran = dbConnection.BeginTransaction())
                {
                    try
                    {
                        Int32 EditRow = base.dbConnection.Execute(EditSQl, null, Tran);
                        Int32 RecordId = base.dbConnection.Execute(RecordSql, null, Tran);
                        if (EditRow == RecordId && EditRow == 1)
                        {
                            Tran.Commit();
                            return new MyResult { Data = true };
                        }
                        Tran.Rollback();
                        return result.SetStatus(ErrorCode.InvalidData, "账户变更发生错误");
                    }
                    catch (Exception ex)
                    {
                        Tran.Rollback();
                        Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                        return result.SetStatus(ErrorCode.InvalidData, "发生错误");
                    }
                    finally { if (base.dbConnection.State == ConnectionState.Open) { base.dbConnection.Close(); } }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug($"钱包账户余额变更发生错误\r\n修改语句：\r\n{EditSQl}\r\n记录语句：{RecordSql}", ex);
                return result.SetStatus(ErrorCode.InvalidData, "发生错误");
            }
            finally
            {
                if (null != CacheLock) { CacheLock.Unlock(); }
            }
        }

        public MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd, int userId)
        {
            MyResult result = new MyResult();
            if (string.IsNullOrEmpty(oldPwd) || string.IsNullOrEmpty(newPwd))
            {
                return result.SetStatus(ErrorCode.InvalidData, "密码不能为空");
            }
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            //查询oldPwd
            var userOldPwd = base.dbConnection.QueryFirstOrDefault<string>($"select `password` from user where id={userId}");
            var _oldPwd = SecurityUtil.MD5(oldPwd);
            if (userOldPwd != _oldPwd)
            {
                return result.SetStatus(ErrorCode.InvalidPassword, "旧密码输入错误");
            }
            var _newPwd = SecurityUtil.MD5(newPwd);
            //更新旧密码
            base.dbConnection.Execute($"update user set `password`='{_newPwd}' where id={userId}");
            result.Data = true;
            return result;
        }

        public async Task<MyResult<object>> ModifyOtcPwd(ModifyOtcPwdDto model, int userId)
        {
            MyResult result = new MyResult();
            if (userId < 0)
            {
                return result.SetStatus(ErrorCode.ErrorSign, "Error Sign");
            }
            if (string.IsNullOrEmpty(model.NewTradePwd))
            {
                return result.SetStatus(ErrorCode.InvalidData, "交易密码不能为空");
            }
            var Key = $"ModifyOtcPwd:{userId}";
            if (RedisCache.Exists(Key))
            {
                return result.SetStatus(ErrorCode.InvalidData, "暂时无法修改密码,稍后再试");
            }

            ConfirmVcode confirmVcode = new ConfirmVcode
            {
                Mobile = model.Mobile,
                Vcode = model.VCode,
                MsgId = model.MsgId
            };

            #region 修改交易密码验证码验证方式
            MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.VCode });

            if (!VerifyRult.Data.Is_Valid)
            {
                return result.SetStatus(ErrorCode.NotFound, "验证码错误");
            }
            #endregion

            //修改密码
            var newTradePwd = SecurityUtil.MD5(model.NewTradePwd);
            base.dbConnection.Execute($"update user set `tradePwd`='{newTradePwd}' where id={userId}");
            result.Data = true;
            RedisCache.Set(Key, userId, 300);
            return result;
        }
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        private async Task<string> UploadQCloudPic(string base64Url, string type, int userId)
        {
            //上传图片
            try
            {
                String BasePic = base64Url;
                var fileName = DateTime.Now.GetTicket().ToString();
                String FilePath = PathUtil.Combine("LFAuth", type, userId.ToString(), Guid.NewGuid().ToString("N") + ".png");
                Regex reg1 = new Regex("%2B", RegexOptions.IgnoreCase);
                Regex reg2 = new Regex("%2F", RegexOptions.IgnoreCase);
                Regex reg3 = new Regex("%3D", RegexOptions.IgnoreCase);
                Regex reg4 = new Regex("(data:([^;]*);base64,)", RegexOptions.IgnoreCase);

                var newBase64 = reg1.Replace(BasePic, "+");
                newBase64 = reg2.Replace(newBase64, "/");
                newBase64 = reg3.Replace(newBase64, "=");
                BasePic = reg4.Replace(newBase64, "");

                byte[] bt = Convert.FromBase64String(BasePic);
                await QCloudSub.PutObject(FilePath, new System.IO.MemoryStream(bt));
                return FilePath;
            }
            catch (Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Debug(ex, ex.Message);
                return "";
            }
        }
        /// <summary>
        /// 录入人工审核信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> AdminAuth(AuthenticationDto model, int userId)
        {
            MyResult result = new MyResult();
            Regex reg = new Regex(@"[\u4e00-\u9fa5]");
            if (reg.IsMatch(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝号不能包含中文");
            }
            if (!ProcessSqlStr(model.Alipay))
            {
                return result.SetStatus(ErrorCode.InvalidData, "支付宝号禁止使用特殊符号");
            }
            var keyFlag = MemoryCacheUtil.Get($"AdminAuth_{userId}");
            if (keyFlag == null)
            {
                MemoryCacheUtil.Set($"AdminAuth_{userId}", 3000, 3);
                try
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(model.PositiveUrl) && model.PositiveUrl.Length > 1000)
                        {
                            model.PositiveUrl = await UploadQCloudPic(model.PositiveUrl, Constants.USER_PIC, userId);
                        }
                        if (!string.IsNullOrEmpty(model.NegativeUrl) && model.NegativeUrl.Length > 1000)
                        {
                            model.NegativeUrl = await UploadQCloudPic(model.NegativeUrl, Constants.USER_PIC, userId);
                        }
                        if (!string.IsNullOrEmpty(model.CharacterUrl) && model.CharacterUrl.Length > 1000)
                        {
                            model.CharacterUrl = await UploadQCloudPic(model.CharacterUrl, Constants.USER_PIC, userId);
                            // var fileName = DateTime.Now.GetTicket().ToString();
                            // model.CharacterUrl = ImageHandlerUtil.SaveBase64Image(model.CharacterUrl, $"{fileName}.png", Constants.USER_PIC);
                        }
                    }
                    catch (System.Exception)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "系统错误请联系管理员");
                    }
                    var userInfo = base.dbConnection.Execute($"select count(id) count from `authentication_infos` where `idNum`='{model.IdNum}'");
                    if (userInfo > 0)
                    {
                        return result.SetStatus(ErrorCode.InvalidData, "身份证号已存在");
                    }
                    base.dbConnection.Open();
                    using (IDbTransaction transaction = dbConnection.BeginTransaction())
                    {
                        try
                        {
                            //查询
                            var isHaveAuth = base.dbConnection.QueryFirstOrDefault<int>($"select id from `authentication_infos` where userId={userId}", null, transaction);
                            if (isHaveAuth > 0)
                            {
                                var updateSql = $"update `authentication_infos` set trueName='{model.TrueName}',pic='{model.PositiveUrl}',pic1='{model.NegativeUrl}',pic2='{model.CharacterUrl}',idNum='{model.IdNum}' where userId={userId}";
                                var res = base.dbConnection.Execute(updateSql, null, transaction);
                                //更新用户审核状态
                                base.dbConnection.Execute($"update user set `auditState`=1,`alipay`='{model.Alipay}' where id={userId}", null, transaction);
                                if (res <= 0)
                                {
                                    return result.SetStatus(ErrorCode.InvalidData, "提交系统异常...");
                                }
                                transaction.Commit();
                            }
                            else
                            {
                                var res = base.dbConnection.Execute($"insert into `authentication_infos`(userId,trueName,pic,pic1,pic2,idNum,authType) values({userId},'{model.TrueName}','{model.PositiveUrl}','{model.NegativeUrl}','{model.CharacterUrl}','{model.IdNum}',1)", null, transaction);
                                //更新用户审核状态
                                base.dbConnection.Execute($"update user set `auditState`=1,`alipay`='{model.Alipay}' where id={userId}", null, transaction);
                                if (res <= 0)
                                {
                                    return result.SetStatus(ErrorCode.InvalidData, "提交系统异常...");
                                }
                                transaction.Commit();
                            }
                        }
                        catch (System.Exception ex)
                        {
                            LogUtil<SystemService>.Error(ex.Message);
                            transaction.Rollback();
                            return result.SetStatus(ErrorCode.SystemError, "提交系统异常...");
                        }
                    }
                    base.dbConnection.Close();
                    return result;
                }
                catch (System.Exception ex)
                {
                    LogUtil<SystemService>.Error(ex.Message);
                    return result.SetStatus(ErrorCode.InvalidData, "请联系管理");
                }
            }
            else
            {
                return result.SetStatus(ErrorCode.InvalidData, "点评率太高...");
            }
        }

        public MyResult<object> LookAdGetCandyP(int AdId, int userId)
        {
            MyResult result = new MyResult();
            return result;
        }

        public async Task<MyResult<object>> ForgetLoginPwd(ConfirmVcode model, int userId)
        {
            MyResult result = new MyResult();
            if (model == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法数据");
            }
            if (string.IsNullOrEmpty(model.Password))
            {
                return result.SetStatus(ErrorCode.InvalidData, "密码不能为空");
            }
            if (!ProcessSqlStr(model.Mobile))
            {
                return result.SetStatus(ErrorCode.InvalidData, "非法操作");
            }
            try
            {
                MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode() { Mobile = model.Mobile, MsgId = model.MsgId, Vcode = model.Vcode });
                if (!VerifyRult.Data.Is_Valid)
                {
                    return result.SetStatus(ErrorCode.NotFound, "验证码错误");
                }

                var enChangePassword = SecurityUtil.MD5(model.Password);
                StringBuilder UpdateSql = new StringBuilder();
                UpdateSql.Append("UPDATE `user` SET `password` = @Password, utime = now() WHERE `mobile` = @Mobile;");
                DynamicParameters UpdateParam = new DynamicParameters();
                UpdateParam.Add("Password", enChangePassword, DbType.String);
                UpdateParam.Add("Mobile", model.Mobile, DbType.String);
                await base.dbConnection.ExecuteAsync(UpdateSql.ToString(), UpdateParam);
            }
            catch (System.Exception ex)
            {
                LogUtil<YoyoUserSerivce>.Error(ex, JsonConvert.SerializeObject(model));
                return result.SetStatus(ErrorCode.SystemError, "系统错误 请稍后再试");
            }
            return result;
        }

        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="unbind"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> UnbindDevice(UnbindDto unbind)
        {
            MyResult<object> result = new MyResult<object>();
            Decimal DeductCandy = 0.50M;
            Int32 MaxUnbind = 10;
            Boolean IsFail = true;

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("Mobile", unbind.Mobile, DbType.String);

            User UserInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>("SELECT `id`, `password`, `status` FROM user WHERE mobile = @Mobile;", QueryParam);
            if (UserInfo == null) { return result.SetStatus(ErrorCode.NotFound, "该账户未注册"); }
            if (!UserInfo.Password.Equals(SecurityUtil.MD5(unbind.Password))) { return result.SetStatus(ErrorCode.NotFound, "密码错误"); }
            if (UserInfo.Status == 2) { return result.SetStatus(ErrorCode.Forbidden, "该账号违规,请联系管理员"); }

            MyResult<MsgDto> VerifyRult = await CheckVcode(new ConfirmVcode()
            {
                Mobile = unbind.Mobile,
                MsgId = unbind.MsgId,
                Vcode = unbind.VerifyCode
            });
            if (!VerifyRult.Data.Is_Valid) { return result.SetStatus(ErrorCode.NotFound, "验证码错误"); }

            Int32 UnbindCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>("SELECT unLockCount FROM login_history WHERE mobile = @Mobile;", QueryParam);
            if (UnbindCount >= MaxUnbind) { return result.SetStatus(ErrorCode.Forbidden, "解绑超出限制,请联系管理员"); }

            QueryParam.Add("UserId", UserInfo.Id, DbType.Int64);
            QueryParam.Add("DeviceId", unbind.DeviceId, DbType.String);
            QueryParam.Add("DeductCandy", DeductCandy, DbType.Decimal);
            QueryParam.Add("CandyDesc", $"设备解绑扣除糖果:{DeductCandy.ToString("0.00")}", DbType.String);
            QueryParam.Add("Source", 13, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    StringBuilder UnbindSql = new StringBuilder();
                    UnbindSql.Append("UPDATE login_history SET systemName = '', uniqueId = '0', unLockCount =( unLockCount + 1 ) WHERE mobile = @Mobile;");
                    UnbindSql.Append("UPDATE login_history SET systemName = '', uniqueId = '0' WHERE uniqueId = @DeviceId;");
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
            result.Data = new object { };
            return result;
        }

        /// <summary>
        /// 用户资产详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> WalletlList(int id)
        {
            MyResult result = new MyResult();
            var walletData = await base.dbConnection.QueryAsync($"select * from user_account_wallet where UserId={id}");
            result.Data = walletData;
            return result;
        }

        /// <summary>
        /// 币种流水
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> WalletlRecordList(WalletRcordsDto model)
        {
            MyResult<object> result = new MyResult<object>();
            var accountSql = "";
            var mobileSql = "";
            var coinTypeSql = "";
            var channelSql = "";
            var modifyTime = "";
            if (model.AccountId != null)
            {
                accountSql += $" and uaw.`AccountId`={model.AccountId}";
            }
            if (model.Mobile != null)
            {
                mobileSql += $" and u.`mobile`='{model.Mobile}'";
            }
            if (model.Channel != null && !string.IsNullOrWhiteSpace(model.Channel))
            {
                channelSql += $" and uawr.`ModifyType`='{model.Channel}'";
            }
            if (model.ModifyTime != null)
            {
                modifyTime += $" and uawr.`ModifyTime`='{model.ModifyTime}'";
            }
            if (model.CoinType != null)
            {
                if (model.CoinType == "0")
                {
                    model.CoinType = "LF";
                }
                else if (model.CoinType == "1")
                {
                    model.CoinType = "糖果";
                }
                else if (model.CoinType == "2")
                {
                    model.CoinType = "钻石";
                }
                else if (model.CoinType == "3")
                {
                    model.CoinType = "USDT(ERC20)";
                }
                else if (model.CoinType == "4")
                {
                    model.CoinType = "BTC";
                }
                else if (model.CoinType == "5")
                {
                    model.CoinType = "ETH";
                }
                else if (model.CoinType == "6")
                {
                    model.CoinType = "YB";
                }
                else
                {
                    return result.SetStatus(ErrorCode.InvalidData, "数据非法");
                }
                coinTypeSql += $" and uaw.`CoinType`='{model.CoinType}'";
            }

            String SelectSql = $"select a.*,u.name,u.mobile from (select uawr.*,uaw.`CoinType`,uaw.`UserId` from `user_account_wallet_record` uawr left join `user_account_wallet`  uaw on uaw.`AccountId`=uawr.`AccountId` where 1=1{accountSql}{channelSql}{modifyTime}{coinTypeSql}) a left join user u on a.`UserId`=u.id where 1=1{mobileSql} order by a.recordId desc";

            var Records = (await base.dbConnection.QueryAsync<UserAccountWalletRecordDto>(SelectSql)).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);

            Records.ToList().ForEach(UserAccountWalletRecord =>
            {
                UserAccountWalletRecord.ModifyDesc = String.Format(UserAccountWalletRecord.ModifyType.GetDescription(), UserAccountWalletRecord.ModifyDesc.Split(","));
            });
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = Records;
            return result;
        }


        public async Task<MyResult<object>> Datasync(WalletRcordsDto model)
        {
            MyResult<object> result = new MyResult<object>();
            var mobileSql = "";
            var coinTypeSql = "";

            if (model.Mobile != null)
            {
                mobileSql += $" and u.`mobile`='{model.Mobile}'";
            }

            if (model.CoinType != null)
            {
                coinTypeSql = $" and CoinType='{model.CoinType}'";
            }

            String SelectSql = $"select a.*,ui.`trueName` from (select a.*,u.name,u.mobile from (select * from `user_account_wallet` where 1=1{coinTypeSql} and `UserId` not in (2,1,3,4,4971,4970,1414,4714,5167,4715,4713,6656,6655)) a left join user u on a.UserId=u.id) a left join `authentication_infos` ui on a.UserId=ui.UserId order by a.Balance desc;";

            var Records = (await base.dbConnection.QueryAsync(SelectSql)).AsQueryable().Pages(model.PageIndex, model.PageSize, out int count, out int pageCount);
            var sumSql = $"select sum(`Balance`) LFTotal from `user_account_wallet` where 1=1{coinTypeSql};";
            var totalSumCoin = base.dbConnection.QueryFirstOrDefault<decimal>(sumSql);
            result.PageCount = pageCount;
            result.RecordCount = count;
            result.Data = new { totalSumCoin = totalSumCoin, Records = Records };
            return result;
        }
        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        public async Task<ListModel<AdminUserModel>> UserList(QueryUser query)
        {
            ListModel<AdminUserModel> Rult = new ListModel<AdminUserModel>()
            {
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            StringBuilder CuntSql = new StringBuilder();
            CuntSql.Append("SELECT COUNT(id) FROM `user` WHERE cCount = 0 AND password <> '1234567' ");

            StringBuilder QuerySql = new StringBuilder();
            QuerySql.Append("SELECT u.id, u.uuid,u.`name`, u.mobile, u.`level`, u.candyNum, u.candyP, u.freezeCandyNum, u.inviterMobile, u.alipay, u.alipayUid, ");
            QuerySql.Append("u.passwordSalt, u.`status`, u.auditState, u.ctime FROM `user` AS u ");
            QuerySql.Append("WHERE u.cCount = 0 AND u.`password` <> '1234567' ");

            DynamicParameters QueryParam = new DynamicParameters();
            QueryParam.Add("UserId", query.CurrentId, DbType.String);
            QueryParam.Add("Mobile", query.Mobile, DbType.String);
            QueryParam.Add("Alipay", query.Alipay, DbType.String);
            QueryParam.Add("InviterMobile", query.InviterMobile, DbType.String);
            QueryParam.Add("Status", (Int32)query.Status, DbType.Int32);
            QueryParam.Add("AuditState", (Int32)query.AuditState, DbType.Int32);
            QueryParam.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParam.Add("PageSize", query.PageSize, DbType.Int32);

            if (query.CurrentId > 0) { QuerySql.Append("AND u.id = @UserId "); CuntSql.Append("AND id = @UserId "); }
            if (!string.IsNullOrWhiteSpace(query.Mobile)) { QuerySql.Append("AND u.mobile = @Mobile "); CuntSql.Append("AND mobile = @Mobile "); }
            if (!string.IsNullOrWhiteSpace(query.Alipay)) { QuerySql.Append("AND u.alipay = @Alipay "); CuntSql.Append("AND alipay = @Alipay "); }
            if (!string.IsNullOrWhiteSpace(query.InviterMobile)) { QuerySql.Append("AND u.inviterMobile = @InviterMobile "); CuntSql.Append("AND inviterMobile = @InviterMobile "); }
            if (query.Status != UserState.All) { QuerySql.Append("AND u.`status` = @Status "); CuntSql.Append("AND status = @Status "); }
            if (query.AuditState != UserAuthState.All) { QuerySql.Append("AND u.`auditState` = @AuditState "); CuntSql.Append("AND auditState = @AuditState "); }

            Rult.Total = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>(CuntSql.ToString(), QueryParam);

            QuerySql.Append("ORDER BY u.id DESC LIMIT @PageIndex, @PageSize;");

            IEnumerable<AdminUserModel> UserList = await base.dbConnection.QueryAsync<AdminUserModel>(QuerySql.ToString(), QueryParam);
            Rult.List = UserList.ToList();

            return Rult;
        }

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Freeze(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            Param.Add("PasswordSalt", model.PasswordSalt, DbType.String);
            Int32 Rows = await dbConnection.ExecuteAsync("UPDATE `user` SET `status` = 2, PasswordSalt = @PasswordSalt WHERE id = @UserId;", Param);
            if (Rows > 0)
            {
                model.Status = 2;
                return model;
            }
            return null;
        }

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Unfreeze(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            Int32 Rows = await dbConnection.ExecuteAsync("UPDATE `user` SET `status` = 0 WHERE id = @UserId;", Param);
            if (Rows > 0)
            {
                model.Status = 0;
                return model;
            }
            return null;
        }

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <returns></returns>
        public async Task<UserDto> Modify(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            UserDto UserInfo = dbConnection.QueryFirstOrDefault<UserDto>("SELECT * FROM `user` WHERE id = @UserId;", Param);
            if (UserInfo == null) { return null; }

            Int32 Rows = 0;

            StringBuilder SqlStr = new StringBuilder();
            SqlStr.Append("UPDATE `user` SET ");

            if (!string.IsNullOrEmpty(model.Alipay) && UserInfo.Alipay != model.Alipay)
            {
                SqlStr.Append("`alipay` = @Alipay ");
                Param.Add("Alipay", model.Alipay, DbType.String);
                Rows++;
            }

            if (UserInfo.AlipayUid != model.AlipayUid)
            {
                SqlStr.Append("`alipayUid` = @AlipayUid ");
                Param.Add("AlipayUid", model.AlipayUid, DbType.String);
                Rows++;
            }

            if (Rows < 1) { return null; }
            SqlStr.Append("WHERE id = @UserId;");
            Int32 row = await dbConnection.ExecuteAsync(SqlStr.ToString(), Param);

            if (Rows == row) { return UserInfo; }
            return null;
        }

        /// <summary>
        /// 查询认证信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<AuthDto> AuthInfo(UserDto model)
        {
            DynamicParameters Param = new DynamicParameters();
            Param.Add("UserId", model.Id, DbType.Int64);
            return await base.dbConnection.QueryFirstOrDefaultAsync<AuthDto>("SELECT * FROM authentication_infos WHERE userId = @UserId;", Param);
        }

        /// <summary>
        /// 我的活动券
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<ActivityCouponModel>>> ActivityCoupon(QueryActivityCoupon query)
        {
            MyResult<List<ActivityCouponModel>> rult = new MyResult<List<ActivityCouponModel>>();
            query.PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;

            StringBuilder CuntSql = new StringBuilder();
            CuntSql.Append("SELECT COUNT(Id) FROM yoyo_activity_coupon WHERE UserId = @UserId ");

            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters Param = new DynamicParameters();
            QuerySql.Append("SELECT * FROM yoyo_activity_coupon WHERE UserId = @UserId ");
            if (query.State != ActivityCouponState.All)
            {
                CuntSql.Append("AND State = @State ");

                QuerySql.Append("AND State = @State ");
                Param.Add("State", (Int32)query.State, DbType.Int32);
            }
            if (query.Type != CouponType.All)
            {
                CuntSql.Append("AND CouponType = @Type ");

                QuerySql.Append("AND CouponType = @Type ");
                Param.Add("Type", (Int32)query.Type, DbType.Int32);
            }

            rult.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>(CuntSql.ToString(), Param);
            rult.PageCount = (rult.RecordCount + query.PageSize - 1) / query.PageSize;

            QuerySql.Append("ORDER BY Id DESC ");
            QuerySql.Append(" LIMIT @PageIndex, @PageSize;");
            Param.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            Param.Add("PageSize", query.PageSize, DbType.Int32);

            var ListCoupon = await base.dbConnection.QueryAsync<ActivityCouponDto>(QuerySql.ToString(), Param);

            rult.Data = new List<ActivityCouponModel>();
            foreach (var item in ListCoupon)
            {
                rult.Data.Add(new ActivityCouponModel()
                {
                    Id = item.Id,
                    Title = item.CouponType.GetDescription(),
                    CouponType = item.CouponType,
                    EffectiveTime = item.EffectiveTime,
                    ExpireTime = item.ExpireTime,
                    State = item.State,
                    CreateTime = item.CreateTime,
                    Remark = item.Remark
                });
            }

            return rult;
        }

        /// <summary>
        /// 设置联系方式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> SetContact(ContactModel model)
        {
            MyResult<Object> result = new MyResult<object>();
            Int32 rows = 0;

            if (model.UserId < 1) { return result.SetStatus(ErrorCode.InvalidData, "请重新登陆"); }

            String Mobile = await base.dbConnection.QueryFirstOrDefaultAsync<String>("SELECT Mobile FROM `user_expand` WHERE `UserId` = @UserId;", new { model.UserId });

            if (Mobile == null)
            {
                rows = base.dbConnection.Execute("INSERT INTO `user_expand`(`UserId`, `Mobile`, `Wechat`, `CreateTime`) VALUES (@UserId, @Mobile, @WeChat, NOW());", new { model.UserId, model.Mobile, model.WeChat });
            }
            else
            {
                rows = base.dbConnection.Execute("UPDATE `user_expand` SET `Mobile` = @Mobile, `Wechat` = @WeChat WHERE `UserId` = @UserId;", new { model.UserId, model.Mobile, model.WeChat });
            }

            if (rows > 0) { return result; }

            return result.SetStatus(ErrorCode.InvalidData, "设置失败");
        }

        /// <summary>
        /// 解封交易
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PayPwd"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> UnblockTrade(Int64 UserId, String PayPwd)
        {
            MyResult result = new MyResult();
            Decimal PayCandy = AppSetting.TradeUnblockCandy;
            Decimal PayPeel = AppSetting.TradeUnblockPeel;

            if (PayCandy <= 0 || PayPeel <= 0) { return result.SetStatus(ErrorCode.InvalidData, "系统维护中..."); }

            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            User userInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>($"select * from user where id={UserId}");
            if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
            if (SecurityUtil.MD5(PayPwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
            if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
            if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }
            if (userInfo.CandyNum < PayCandy) { return result.SetStatus(ErrorCode.InvalidData, $"解封交易,需要{PayCandy}糖果哦~"); }
            if (userInfo.CandyP < PayPeel) { return result.SetStatus(ErrorCode.InvalidData, $"解封交易,需要{PayPeel}果皮哦~"); }

            StringBuilder QueryBanSql = new StringBuilder();
            QueryBanSql.Append("SELECT * FROM (");
            QueryBanSql.Append("SELECT * FROM coin_trade WHERE buyerUid = @UserId AND buyerBan = 1 ");
            QueryBanSql.Append("UNION ");
            QueryBanSql.Append("SELECT * FROM coin_trade WHERE sellerUid = @UserId AND sellerBan = 1 ");
            QueryBanSql.Append(") AS o ORDER BY o.dealTime DESC;");
            CoinTrade TradeOrder = await dbConnection.QueryFirstOrDefaultAsync<CoinTrade>(QueryBanSql.ToString(), new { UserId });

            if (TradeOrder == null) { return result.SetStatus(ErrorCode.NoAuth, "您的交易很正常哦~"); }

            #region 拼装SQL 并扣款
            //解封交易 1
            StringBuilder UnblockSql = new StringBuilder();
            UnblockSql.Append("UPDATE coin_trade SET buyerBan = 0, sellerBan = 0 WHERE id = @TradeId;");
            DynamicParameters UnblockParam = new DynamicParameters();
            UnblockParam.Add("TradeId", TradeOrder.Id, DbType.Int64);

            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", UserId, DbType.Int64);
            DeductParams.Add("PayCandy", PayCandy, DbType.Decimal);
            DeductParams.Add("PayPeel", PayPeel, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 1
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", PayCandy, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"解封交易扣除: {PayCandy.ToString("0.####")}糖果", DbType.String);
            CandyRecordParams.Add("Source", 32, DbType.Int32);

            //写入 果皮扣除记录 1
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PayPeel, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"解封交易扣除: {PayPeel.ToString("0.####")}果皮", DbType.String);
            PeelRecordParams.Add("Source", 32, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = dbConnection.Execute(UnblockSql.ToString(), UnblockParam, transaction);
                    Rows += dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                    Rows += dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                    Rows += dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                    if (Rows != 4)
                    {
                        throw new Exception("解封交易[S]");
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug(new { UserId, PayPwd }, ex);
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open) { dbConnection.Close(); }
                }
            }
            #endregion
            return result;


        }

        /// <summary>
        /// 修改手机号 （未实现）
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Mobile"></param>
        /// <param name="PayPwd"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> ModifyMobile(long UserId, string Mobile, string PayPwd)
        {
            MyResult result = new MyResult();
            Decimal PayCandy = AppSetting.TradeUnblockCandy;
            Decimal PayPeel = AppSetting.TradeUnblockPeel;

            if (PayCandy <= 0 || PayPeel <= 0) { return result.SetStatus(ErrorCode.InvalidData, "系统维护中..."); }
            if (!DataValidUtil.IsMobile(Mobile)) { return result.SetStatus(ErrorCode.InvalidData, "手机号不合法"); }

            if (UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "Sign Error"); }
            User userInfo = await base.dbConnection.QueryFirstOrDefaultAsync<User>($"select * from user where id={UserId}");
            if (userInfo == null) { return result.SetStatus(ErrorCode.InvalidData, "用户信息不存在..."); }
            if (SecurityUtil.MD5(PayPwd) != userInfo.TradePwd) { return result.SetStatus(ErrorCode.InvalidPassword, "交易密码有误"); }
            if (userInfo.AuditState != 2) { return result.SetStatus(ErrorCode.NoAuth, "没有实名认证"); }
            if (userInfo.Status == 2 || userInfo.Status == 3 || userInfo.Status == 5) { return result.SetStatus(ErrorCode.AccountDisabled, "账号异常 请联系管理员"); }
            if (userInfo.CandyNum < PayCandy) { return result.SetStatus(ErrorCode.InvalidData, $"修改手机号,需要{PayCandy}糖果哦~"); }
            if (userInfo.CandyP < PayPeel) { return result.SetStatus(ErrorCode.InvalidData, $"修改手机号,需要{PayPeel}果皮哦~"); }

            if (userInfo.Mobile.Equals(Mobile, StringComparison.OrdinalIgnoreCase)) { return result.SetStatus(ErrorCode.InvalidData, "当前手机号无需修改~"); }
            User MobileUser = await base.dbConnection.QueryFirstOrDefaultAsync<User>($"SELECT * FROM `user` WHERE mobile = @Mobile;", new { Mobile });
            if (MobileUser != null) { return result.SetStatus(ErrorCode.InvalidData, $"手机号{Mobile},已被使用~"); }

            #region 拼装SQL 并扣款
            String CurrentMobile = userInfo.Mobile;
            //修改手机号 1 + N
            StringBuilder ModifySql = new StringBuilder();
            ModifySql.Append("UPDATE `user` SET `mobile` = @Mobile WHERE `id` = @UserId;");
            ModifySql.Append("UPDATE `user` SET `inviterMobile` = @Mobile WHERE `inviterMobile` = @CurrentMobile");
            DynamicParameters ModifyParam = new DynamicParameters();
            ModifyParam.Add("UserId", UserId, DbType.Int64);
            ModifyParam.Add("Mobile", Mobile, DbType.String);
            ModifyParam.Add("CurrentMobile", CurrentMobile, DbType.String);

            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", UserId, DbType.Int64);
            DeductParams.Add("PayCandy", PayCandy, DbType.Decimal);
            DeductParams.Add("PayPeel", PayPeel, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 1
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", PayCandy, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"修改手机号: {PayCandy.ToString("0.####")}糖果", DbType.String);
            CandyRecordParams.Add("Source", 32, DbType.Int32);

            //写入 果皮扣除记录 1
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PayPeel, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"修改手机号: {PayPeel.ToString("0.####")}果皮", DbType.String);
            PeelRecordParams.Add("Source", 32, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = dbConnection.Execute(ModifySql.ToString(), ModifyParam, transaction);
                    Rows += dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                    Rows += dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                    Rows += dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                    if (Rows >= 4)
                    {
                        throw new Exception("修改手机号[S]");
                    }
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    SystemLog.Debug($"{UserId}, {Mobile}, {PayPwd}", ex);
                }
                finally
                {
                    if (dbConnection.State == ConnectionState.Open) { dbConnection.Close(); }
                }
            }
            #endregion
            return result;
        }
    }
}