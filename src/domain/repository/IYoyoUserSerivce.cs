using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using domain.models;
using domain.models.admin;
using domain.models.yoyoDto;
using domain.lfexentitys;
using domain.models.lfexDto;

namespace domain.repository
{
    public interface IYoyoUserSerivce
    {
        Task<MyResult<object>> WalletlList(int id);
        Task<MyResult<object>> WalletlRecordList(WalletRcordsDto model);
        Task<MyResult<object>> Datasync(WalletRcordsDto model);
        //登陆
        MyResult<object> Login(YoyoUserDto model);
        //注册
        Task<MyResult<object>> SignUp(SignUpDto model);
        //发送验证码
        // MyResult<object> SendVcode(SendVcode model);
        //校验验证码
        // MyResult<object> ConfirmVcode(ConfirmVcode model);
        //发送验证码
        Task<MyResult<object>> SendVcode(SendVcode model);
        //校验验证码
        //Task<MyResult<object>> ConfirmVcode(ConfirmVcode model);
        // 获取用户信息--昵称和头像
        MyResult<User> GetNameByRcode(string mobile);
        MyResult<object> GetNameByMobile(string mobile);
        /// <summary>
        /// 根据手机号 获取会员信息
        /// </summary>
        /// <param name="mobile"></param>
        /// <returns></returns>
        Task<MyResult<object>> GetUserByMobile(string mobile);
        //获取用户钱包金额
        MyResult<object> WalletAmount(int userId);
        /// <summary>
        /// 团队信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <param name="mobile"></param>
        /// <returns></returns>
        Task<MyResult<object>> TeamInfos(TeamInfosReqDto model, int userId, string mobile);
        //更新团队人数
        MyResult<object> UpdateTeamCount();
        //更新实名活跃度
        MyResult<object> UpdateTeamCandyHByAuth();
        //根据任务添加活跃度
        MyResult<object> AddTeamCandyHByTask();
        //根据过期任务减活跃度
        MyResult<object> SubTeamCandyHByTask();
        //生成支付订单签名
        Task<MyResult<object>> GenerateAppUrl(int userId);

        /// <summary>
        /// 实名广告
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<object>> RealNameAd(Int64 UserId);

        /// <summary>
        /// 实名认证失败，退款
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> PayRefund(int userId);

        //阿里支付异步通知
        Task<string> AliNotify(string TradeNo);
        //标志支付成功
        Task<MyResult<object>> PayFlag(int userId, string outTradeNo);
        //认证
        MyResult<object> Authentication(AuthenticationDto model, int userId);

        /// <summary>
        /// 刷脸认证效验
        /// </summary>
        /// <param name="model"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        Task<MyResult<object>> IsFaceAuth(AuthenticationDto model, int UserId);

        /// <summary>
        /// 扫脸认证【未起用】
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<MyResult<object>> ScanFaceInit(AuthenticationDto model, int userId);

        /// <summary>
        /// 扫脸认证记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task WriteInitRecord(AuthenticationDto model);

        /// <summary>
        /// 扫脸认证记录校验
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<FaceInitRecord> VerfyFaceInit(AuthenticationDto model);

        //修改头像
        MyResult<object> ModifyUserPic(YoyoUserDto model, int userId);
        //修改昵称
        MyResult<object> ModifyUserName(string name, int userId);
        Task<MyResult<object>> ModifyUserInviterCode(string name, int userId);

        //修改登陆密码
        MyResult<object> ModifyLoginPwd(string oldPwd, string newPwd, int userId);
        //修改交易密码
        // MyResult<object> ModifyOtcPwd(ModifyOtcPwdDto model, int userId);
        Task<MyResult<object>> ModifyOtcPwd(ModifyOtcPwdDto model, int userId);
        //人工审核
        Task<MyResult<object>> AdminAuth(AuthenticationDto model, int userId);
        //
        MyResult<object> LookAdGetCandyP(int AdId, int userId);
        //忘记密码
        Task<MyResult<object>> ForgetLoginPwd(ConfirmVcode model, int userId);
        //查看是否存在订单
        Task<MyResult<object>> HavePayOrder(int userId);

        /// <summary>
        /// 设置联系方式
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<MyResult<Object>> SetContact(ContactModel model);

        /// <summary>
        /// 我的活动券
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        Task<MyResult<List<ActivityCouponModel>>> ActivityCoupon(QueryActivityCoupon query);

        /// <summary>
        /// 解封交易
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PayPwd"></param>
        /// <returns></returns>
        Task<MyResult<Object>> UnblockTrade(Int64 UserId, String PayPwd);

        /// <summary>
        /// 修改手机号
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Mobile"></param>
        /// <param name="PayPwd"></param>
        /// <returns></returns>
        Task<MyResult<Object>> ModifyMobile(Int64 UserId, String Mobile, String PayPwd);

        #region 后台管理
        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="unbind"></param>
        /// <returns></returns>
        Task<MyResult<object>> UnbindDevice(UnbindDto unbind);

        /// <summary>
        /// 会员列表
        /// </summary>
        /// <returns></returns>
        Task<ListModel<AdminUserModel>> UserList(QueryUser query);

        /// <summary>
        /// 冻结会员
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Freeze(UserDto query);

        /// <summary>
        /// 解冻会员
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Unfreeze(UserDto query);

        /// <summary>
        /// 修改会员信息
        /// </summary>
        /// <returns></returns>
        Task<UserDto> Modify(UserDto query);

        /// <summary>
        /// 查询认证信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<AuthDto> AuthInfo(UserDto model);
        #endregion

    }
}