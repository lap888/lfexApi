using application.Request;
using application.Response;
using application.Utils;
using CSRedis;
using Dapper;
using domain.configs;
using domain.enums;
using domain.models;
using domain.repository;
using domain.lfexentitys;
using infrastructure.extensions;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using domain.models.lfexDto;

namespace application.services
{
    /// <summary>
    /// 充值
    /// </summary>
    public class RechargeService : bases.BaseServiceLfex, IRechargeService
    {
        private readonly IFuluRecharge Recharge;
        private readonly CSRedisClient RedisCache;
        private readonly Models.AppSetting Settings;
        public RechargeService(IFuluRecharge recharge, CSRedisClient cSRedis, IOptionsMonitor<Models.AppSetting> monitor, IOptionsMonitor<ConnectionStringList> connectionStringList) : base(connectionStringList)
        {
            Recharge = recharge;
            RedisCache = cSRedis;
            Settings = monitor.CurrentValue;
        }
        /// <summary>
        /// 获取手机号信息
        /// </summary>
        /// <param name="Phone"></param>
        /// <returns></returns>
        public async Task<MyResult<PhoneInfoModel>> MobileInfo(string Phone)
        {
            MyResult<PhoneInfoModel> result = new MyResult<PhoneInfoModel>();
            if (!DataValidUtil.IsMobile(Phone)) { return result.SetStatus(ErrorCode.InvalidData, "手机号码有误"); }
            var Rult = await Recharge.Execute(new ReqMobileInfo() { Phone = Phone });
            if (Rult.IsError)
            {
                return result.SetStatus(ErrorCode.InvalidData, Rult.Message);
            }
            if (Rult.ResultData?.FaceValue == null)
            {
                return result.SetStatus(ErrorCode.InvalidData, "暂不支持此手机号充值");
            }
            result.Data = new PhoneInfoModel()
            {
                Operator = Rult.ResultData.Operator,
                Province = Rult.ResultData.Province,
                City = Rult.ResultData.City,
                CityCode = Rult.ResultData.CityCode,
                SpType = Rult.ResultData.SpType,
                FaceValue = new List<FaceValueModel>()
            };
            #region 取平台指导价
            Decimal SysPrice = base.dbConnection.QueryFirstOrDefault<Decimal>("SELECT (`sysMinPrice` + `sysMaxPrice`) / 2 FROM coin_trade_ext ORDER BY id DESC LIMIT 1;");
            if (SysPrice < 1) { return result.SetStatus(ErrorCode.InvalidData, "充值服务维护中"); }
            #endregion
            foreach (var item in Rult.ResultData.FaceValue.OrderBy(item => item))
            {
                result.Data.FaceValue.Add(new FaceValueModel()
                {
                    FaceValue = item.ToString("0.##"),
                    CandyNum = (Int32)(item / SysPrice * 10000) / 10000M
                });
            }
            return result;
        }

        /// <summary>
        /// 充值
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> MobileRecharge(PhoneRechargeModel model)
        {
            MyResult<Object> result = new MyResult<object>();
            Boolean IsSuccess = false;
            Decimal TodayTotal = base.dbConnection.QueryFirstOrDefault<Decimal?>("SELECT SUM(Price) FROM yoyo_recharge_order WHERE TO_DAYS(CreateTime) = TO_DAYS(NOW()) AND State != 0;") ?? 0;
            if (TodayTotal > 300) { return result.SetStatus(ErrorCode.InvalidData, "活动过于火爆,明日再来吧"); }

            FuluResponse<RspFuluBalance> FuluInfo = await Recharge.Execute(new ReqFuluBalance());

            if (FuluInfo?.ResultData == null || FuluInfo.ResultData.Balance < 200 || !FuluInfo.ResultData.IsOpen)
            {
                return result.SetStatus(ErrorCode.InvalidData, "活动过于火爆,明日再来吧");
            }

            #region 基础验证 
            Int32 Level = 0;
            if (!DataValidUtil.IsMobile(model.Phone)) { return result.SetStatus(ErrorCode.InvalidData, "手机号码有误"); }
            if (model.UserId < 1) { return result.SetStatus(ErrorCode.ErrorSign, "sign error"); }
            if (model.FaceValue < 10 || model.FaceValue > 500) { return result.SetStatus(ErrorCode.InvalidData, "请求参数"); }
            var UserInfo = base.dbConnection.QueryFirstOrDefault<User>("SELECT * FROM `user` WHERE id = @UserId;", new { UserId = model.UserId });
            if (!Int32.TryParse(UserInfo.Level.Replace("lv", ""), out Level) || Level < 2)
            {
                return result.SetStatus(ErrorCode.InvalidData, "会员lv2及以上才能体验哦");
            }
            if (Level > 8) { return result.SetStatus(ErrorCode.InvalidData, "别闹了,留给会员体验吧"); }
            if (!UserInfo.TradePwd.Equals(SecurityUtil.MD5(model.PayPwd))) { return result.SetStatus(ErrorCode.InvalidData, "支付密码错误"); }
            Int32 RecCount = base.dbConnection.QueryFirstOrDefault<Int32>("SELECT COUNT(Id) FROM yoyo_recharge_order WHERE UserId = @UserId AND DATE_FORMAT(CreateTime,'%Y%m') = DATE_FORMAT(NOW(),'%Y%m');", new { UserId = model.UserId });
            if (RecCount > 0) { return result.SetStatus(ErrorCode.InvalidData, "体验期间每月仅限一次"); }
            #endregion

            #region 计算 需要支付的糖果数
            Models.UserLevel UserLevel = Settings.Levels.Where(item => item.Level.Equals(UserInfo.Level, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            Decimal SysPrice = base.dbConnection.QueryFirstOrDefault<Decimal>("SELECT `sysMinPrice` FROM coin_trade_ext ORDER BY id DESC LIMIT 1;");
            Decimal PayCandy = (Int32)(model.FaceValue / SysPrice * 10000) / 10000M;
            Decimal PayPeel = PayCandy;
            Decimal PayFee = PayCandy * UserLevel.SellRate - PayCandy;
            if (UserInfo.CandyNum < (PayCandy + PayFee)) { return result.SetStatus(ErrorCode.InvalidData, "糖果数量不足"); }
            if (UserInfo.CandyP < (PayPeel + PayFee)) { return result.SetStatus(ErrorCode.InvalidData, "果皮数量不足"); }
            #endregion

            String OrderNo = Gen.NewGuid20();

            #region 拼装SQL 并扣款
            //写入充值记录 1
            StringBuilder RecSql = new StringBuilder();
            DynamicParameters RecParam = new DynamicParameters();
            RecParam.Add("UserId", model.UserId, DbType.Int64);
            RecParam.Add("PayCandy", PayCandy + PayFee, DbType.Decimal);
            RecParam.Add("PayPeel", PayPeel + PayFee, DbType.Decimal);
            RecSql.Append("INSERT INTO yoyo_recharge_order ");
            RecSql.Append("( OrderNo, OrderType, UserId, FaceValue, Account, Price, PayCandy, PayPeel, State ) ");
            RecSql.Append("VALUES ( @OrderNo, @OrderType, @UserId, @FaceValue, @Account, @Price, @PayCandy, @PayPeel, @State );");
            RecParam.Add("OrderNo", OrderNo, DbType.String);
            RecParam.Add("OrderType", 1, DbType.Int32);
            RecParam.Add("FaceValue", model.FaceValue.ToString(), DbType.String);
            RecParam.Add("Price", model.FaceValue, DbType.Decimal);
            RecParam.Add("Account", model.Phone, DbType.String);
            RecParam.Add("State", RechargeState.PROCESSING, DbType.String);

            //扣除 账户 1
            StringBuilder DeductSql = new StringBuilder();
            DynamicParameters DeductParams = new DynamicParameters();
            DeductParams.Add("UserId", model.UserId, DbType.Int64);
            DeductParams.Add("PayCandy", PayCandy + PayFee, DbType.Decimal);
            DeductParams.Add("PayPeel", PayPeel + PayFee, DbType.Decimal);
            DeductSql.Append("UPDATE `user` SET candyNum = candyNum - @PayCandy, candyP = candyP - @PayPeel ");
            DeductSql.Append("WHERE id = @UserId AND candyNum >= @PayCandy AND candyP >= @PayPeel;");

            //写入 糖果扣除记录 2
            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, -@PayCandy, NOW(), NOW(), @CandyDesc, @Source), ");
            CandyRecordSql.Append("(@UserId, -@PayFee, NOW(), NOW(), @FeeDesc, @Source); ");
            CandyRecordParams.Add("UserId", model.UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", PayCandy, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"手机充值扣除: {PayCandy.ToString("0.####")}糖果", DbType.String);
            CandyRecordParams.Add("PayFee", PayFee, DbType.Decimal);
            CandyRecordParams.Add("FeeDesc", $"手机充值手续费: {PayFee.ToString("0.####")}糖果", DbType.String);
            CandyRecordParams.Add("Source", 21, DbType.Int32);

            //写入 果皮扣除记录 2
            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, -@PayPeel, @PeelDesc, @Source, NOW(), NOW()), ");
            PeelRecordSql.Append("(@UserId, -@PayFee, @FeeDesc, @Source, NOW(), NOW()); ");
            PeelRecordParams.Add("UserId", model.UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", PayPeel, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"手机充值扣除: {PayPeel.ToString("0.####")}果皮", DbType.String);
            PeelRecordParams.Add("PayFee", PayFee, DbType.Decimal);
            PeelRecordParams.Add("FeeDesc", $"手机充值手续费: {PayFee.ToString("0.####")}果皮", DbType.String);
            PeelRecordParams.Add("Source", 21, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = dbConnection.Execute(RecSql.ToString(), RecParam, transaction);
                    Rows += dbConnection.Execute(DeductSql.ToString(), DeductParams, transaction);
                    Rows += dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                    Rows += dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);
                    if (Rows != 6)
                    {
                        throw new Exception("扣款失败[S]");
                    }
                    transaction.Commit();
                    IsSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Yoyo.Core.SystemLog.Debug(model.GetJson(), ex);
                    IsSuccess = false;
                }
            }
            base.dbConnection.Close();
            #endregion

            if (IsSuccess)
            {
                FuluResponse<RspMobileRecharge> Rult = await Recharge.Execute(new ReqMobileRecharge() { ChargePhone = model.Phone, ChargeValue = model.FaceValue, CustomerOrderNo = OrderNo });
                if (Rult.IsError)
                {
                    Yoyo.Core.SystemLog.Debug($"{model.GetJson()}\r\n{Rult.Result}\r\n");
                }
                return result;
            }
            return result.SetStatus(ErrorCode.InvalidData, "充值失败");
        }

        /// <summary>
        /// 订单三方查询
        /// </summary>
        /// <param name="OrderNo"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> QueryOrder(string OrderNo)
        {
            MyResult<object> result = new MyResult<object>();
            Int32 Rows = 0;
            FuluResponse<RspRechargeQuery> Rult = await Recharge.Execute(new ReqRechargeQuery() { CustomerOrderNo = OrderNo });
            if (Rult.IsError)
            {
                Yoyo.Core.SystemLog.Debug(Rult.GetJson());
                return result.SetStatus(ErrorCode.InvalidData, Rult.Message);
            }
            try
            {
                StringBuilder UpdateSql = new StringBuilder();
                DynamicParameters UpdateParam = new DynamicParameters();
                UpdateSql.Append("UPDATE yoyo_recharge_order SET ChannelNo = @ChannelNo, ProductId = @ProductId, ProductName = @ProductName, ");
                UpdateSql.Append("Price = @Price, BuyNum = @BuyNum, PurchasePrice = @PurchasePrice, State = @State, UpdateTime =@UpdateTime ");
                UpdateSql.Append("WHERE OrderNo = @OrderNo AND State != @SuccessState AND State != @RefundState;");
                UpdateParam.Add("OrderNo", OrderNo, DbType.String);
                UpdateParam.Add("ChannelNo", Rult.ResultData.OrderId, DbType.String);
                UpdateParam.Add("ProductId", Rult.ResultData.ProductId, DbType.String);
                UpdateParam.Add("ProductName", Rult.ResultData.ProductName, DbType.String);
                UpdateParam.Add("Price", Rult.ResultData.OrderPrice, DbType.Decimal);
                UpdateParam.Add("BuyNum", Rult.ResultData.BuyNum, DbType.Int32);
                UpdateParam.Add("PurchasePrice", Rult.ResultData.OrderPrice, DbType.Int32);
                UpdateParam.Add("UpdateTime", DateTime.Now, DbType.DateTime);
                UpdateParam.Add("SuccessState", (Int32)RechargeState.SUCCESS, DbType.Int32);
                UpdateParam.Add("RefundState", (Int32)RechargeState.REFUNDED, DbType.Int32);

                switch (Rult.ResultData.OrderState)
                {
                    case Enums.RechargeState.success:
                        UpdateParam.Add("State", (Int32)RechargeState.SUCCESS, DbType.Int32);
                        break;
                    case Enums.RechargeState.failed:
                        UpdateParam.Add("State", (Int32)RechargeState.FAILED, DbType.Int32);
                        break;
                    case Enums.RechargeState.untreated:
                        UpdateParam.Add("State", (Int32)RechargeState.UNTREATED, DbType.Int32);
                        break;
                    default:
                        Yoyo.Core.SystemLog.Warn($"状态异常===>>{Rult.GetJson()}");
                        return result.SetStatus(ErrorCode.InvalidData, "充值处理中..");
                }
                Rows = base.dbConnection.Execute(UpdateSql.ToString(), UpdateParam);
                if (Rows > 0)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                Yoyo.Core.SystemLog.Debug(Rult.GetJson(), ex);
            }
            return result.SetStatus(ErrorCode.InvalidData, "充值处理中..");
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<MyResult<List<RechargeOrder>>> RechargeOrder(QueryRechargeOrder query)
        {
            MyResult<List<RechargeOrder>> result = new MyResult<List<RechargeOrder>>();
            query.PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;

            if (!String.IsNullOrWhiteSpace(query.Mobile))
            {
                query.UserId = await base.dbConnection.QueryFirstAsync<Int64>("SELECT id FROM `user` WHERE mobile = @Mobile LIMIT 1;", new { query.Mobile });
            }

            StringBuilder CuntSql = new StringBuilder();
            CuntSql.Append("SELECT COUNT(Id) FROM yoyo_recharge_order WHERE 1 = 1 ");
            StringBuilder QuerySql = new StringBuilder();
            DynamicParameters QueryParams = new DynamicParameters();
            QuerySql.Append("SELECT Id, OrderNo, ChannelNo, UserId, OrderType, ProductId, ProductName, FaceValue, Account, Price, PurchasePrice, BuyNum, PayCandy, PayPeel, State, CreateTime, UpdateTime, Remark ");
            QuerySql.Append("FROM yoyo_recharge_order WHERE 1 = 1 ");

            if (query.UserId > 0)
            {
                CuntSql.Append("AND UserId = @UserId ");

                QuerySql.Append("AND UserId = @UserId ");
                QueryParams.Add("@UserId", query.UserId, DbType.Int64);
            }

            if (!String.IsNullOrWhiteSpace(query.RechargePhone))
            {
                CuntSql.Append("AND Account = @Account ");

                QuerySql.Append("AND Account = @Account ");
                QueryParams.Add("@Account", query.RechargePhone, DbType.String);
            }

            if (query.OrderType > 0)
            {
                CuntSql.Append("AND OrderType = @OrderType ");

                QuerySql.Append("AND OrderType = @OrderType ");
                QueryParams.Add("@OrderType", query.OrderType, DbType.String);
            }

            if (query.State != RechargeState.UNKNOWN)
            {
                CuntSql.Append("AND State = @State ");

                QuerySql.Append("AND State = @State ");
                QueryParams.Add("@State", query.State, DbType.Int32);
            }
            QuerySql.Append("ORDER BY `Id` DESC ");
            QuerySql.Append("LIMIT @PageIndex,@PageSize;");
            QueryParams.Add("PageIndex", (query.PageIndex - 1) * query.PageSize, DbType.Int32);
            QueryParams.Add("PageSize", query.PageSize, DbType.Int32);

            result.RecordCount = await base.dbConnection.QueryFirstOrDefaultAsync<Int32>(CuntSql.ToString(), QueryParams);
            result.PageCount = (result.RecordCount + query.PageSize - 1) / query.PageSize;
            result.Data = (await base.dbConnection.QueryAsync<RechargeOrder>(QuerySql.ToString(), QueryParams)).ToList();

            return result;
        }

        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Refund(RechargeOrder order)
        {
            MyResult<Object> result = new MyResult<object>();
            Boolean IsSuccess = false;
            order = await base.dbConnection.QueryFirstOrDefaultAsync<RechargeOrder>("SELECT * FROM yoyo_recharge_order WHERE Id = @Id;", new { order.Id });

            if (order == null || order.State != RechargeState.FAILED)
            {
                return result.SetStatus(ErrorCode.InvalidData, "此订单不可退款");
            }

            StringBuilder OrderSql = new StringBuilder();
            DynamicParameters OrderParams = new DynamicParameters();
            OrderParams.Add("Id", order.Id, DbType.Int64);
            OrderParams.Add("Remark", "后台退款", DbType.String);
            OrderParams.Add("FailedState", RechargeState.FAILED, DbType.Int32);
            OrderParams.Add("State", RechargeState.REFUNDED, DbType.Int32);
            OrderSql.Append("UPDATE `yoyo_recharge_order` SET State = @State, Remark = @Remark WHERE id = @Id AND State = @FailedState;");

            StringBuilder AddSql = new StringBuilder();
            DynamicParameters AddParams = new DynamicParameters();
            AddParams.Add("UserId", order.UserId, DbType.Int64);
            AddParams.Add("PayCandy", order.PayCandy, DbType.Decimal);
            AddParams.Add("PayPeel", order.PayPeel, DbType.Decimal);
            AddSql.Append("UPDATE `user` SET candyNum = candyNum + @PayCandy, candyP = candyP + @PayPeel WHERE id = @UserId;");

            StringBuilder CandyRecordSql = new StringBuilder();
            DynamicParameters CandyRecordParams = new DynamicParameters();
            CandyRecordSql.Append("INSERT INTO `gem_records`(`userId`, `num`, `createdAt`, `updatedAt`, `description`, `gemSource`) ");
            CandyRecordSql.Append("VALUES (@UserId, @PayCandy, NOW(), NOW(), @CandyDesc, @Source);");
            CandyRecordParams.Add("UserId", order.UserId, DbType.Int64);
            CandyRecordParams.Add("PayCandy", order.PayCandy, DbType.Decimal);
            CandyRecordParams.Add("CandyDesc", $"充值失败退回: {order.PayCandy.ToString("0.####")}糖果", DbType.String);
            CandyRecordParams.Add("Source", 21, DbType.Int32);

            StringBuilder PeelRecordSql = new StringBuilder();
            DynamicParameters PeelRecordParams = new DynamicParameters();
            PeelRecordSql.Append("INSERT INTO `user_candyp`(`userId`, `candyP`, `content`, `source`, `createdAt`, `updatedAt`) ");
            PeelRecordSql.Append("VALUES (@UserId, @PayPeel, @PeelDesc, @Source, NOW(), NOW());");
            PeelRecordParams.Add("UserId", order.UserId, DbType.Int64);
            PeelRecordParams.Add("PayPeel", order.PayPeel, DbType.Decimal);
            PeelRecordParams.Add("PeelDesc", $"充值失败退回: {order.PayPeel.ToString("0.####")}果皮", DbType.String);
            PeelRecordParams.Add("Source", 21, DbType.Int32);

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = 0;
                    Rows += base.dbConnection.Execute(OrderSql.ToString(), OrderParams, transaction);
                    Rows += base.dbConnection.Execute(AddSql.ToString(), AddParams, transaction);
                    Rows += base.dbConnection.Execute(CandyRecordSql.ToString(), CandyRecordParams, transaction);
                    Rows += base.dbConnection.Execute(PeelRecordSql.ToString(), PeelRecordParams, transaction);

                    if (Rows != 4)
                    {
                        throw new Exception("退款失败[S]");
                    }
                    transaction.Commit();
                    IsSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    IsSuccess = false;
                    Yoyo.Core.SystemLog.Debug(ex);
                }
            }
            base.dbConnection.Close();

            if (IsSuccess) { return result; }

            return result.SetStatus(ErrorCode.InvalidData, "系统错误");
        }

        /// <summary>
        /// 人工处理
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<MyResult<Object>> Succeed(RechargeOrder order)
        {
            MyResult<Object> result = new MyResult<object>();
            Boolean IsSuccess = false;
            order = await base.dbConnection.QueryFirstOrDefaultAsync<RechargeOrder>("SELECT * FROM yoyo_recharge_order WHERE Id = @Id;", new { order.Id });

            if (order == null || order.State == RechargeState.SUCCESS || order.State == RechargeState.REFUNDED)
            {
                return result.SetStatus(ErrorCode.InvalidData, "此订单不可人工处理");
            }

            StringBuilder OrderSql = new StringBuilder();
            DynamicParameters OrderParams = new DynamicParameters();
            OrderParams.Add("Id", order.Id, DbType.Int64);
            OrderParams.Add("State", RechargeState.SUCCESS, DbType.Int32);
            OrderParams.Add("Remark", "人工处理", DbType.String);
            OrderParams.Add("FailedState", RechargeState.FAILED, DbType.Int32);
            OrderSql.Append("UPDATE `yoyo_recharge_order` SET State = @State, Remark = @Remark WHERE id = @Id AND State = @FailedState;");

            base.dbConnection.Open();
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    Int32 Rows = base.dbConnection.Execute(OrderSql.ToString(), OrderParams, transaction);

                    if (Rows != 1)
                    {
                        throw new Exception("处理失败[S]");
                    }
                    transaction.Commit();
                    IsSuccess = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    IsSuccess = false;
                    Yoyo.Core.SystemLog.Debug(ex);
                }
            }
            base.dbConnection.Close();

            if (IsSuccess) { return result; }

            return result.SetStatus(ErrorCode.InvalidData, "系统错误");
        }
    }
}
