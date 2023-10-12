// using Microsoft.AspNetCore.Mvc;
// using domain.repository;
// using Microsoft.AspNetCore.Authorization;
// using domain.models.dto;
// using infrastructure.utils;
// using System.Threading.Tasks;
// using application;
// using application.Request;
// using application.Response;
// using CSRedis;
// using yoyoApi.Controllers.Base;
// using domain.models.yoyoDto;
// using System;

// namespace yoyoApi.Controllers
// {
//     [Route("apiV2/[action]")]
//     [Produces("application/json")]
//     public class ApiV2Controller : ApiBaseController
//     {
//         private readonly CSRedisClient RedisCache;
//         private readonly IRealVerify RealVerify;
//         public readonly IYoyoUserSerivce YoyoUserSerivce;

//         public ApiV2Controller(IRealVerify realVerify, IYoyoUserSerivce yoyoUserSerivce, CSRedisClient redisClient)
//         {
//             RealVerify = realVerify;
//             RedisCache = redisClient;
//             YoyoUserSerivce = yoyoUserSerivce;
//         }

//         [HttpGet]
//         [AllowAnonymous]
//         public async Task<MyResult<object>> FaceQuery(string certifyId)
//         {
//             return await Task.Run(() =>
//             {
//                 return new MyResult<object>(-1, "请升级最新版");
//             });
//         }

//         [HttpPost]
//         public async Task<MyResult<object>> FaceInit([FromBody]FaceDto model)
//         {
//             MyResult result = new MyResult();
//             #region 验证请求参数
//             if (base.TokenModel == null)
//             {
//                 return new MyResult<object>(-1, "请重新登录");
//             }
//             if (string.IsNullOrEmpty(model.CertName))
//             {
//                 return new MyResult<object>(-1, "姓名不能为空");
//             }
//             if (string.IsNullOrEmpty(model.CertNo))
//             {
//                 return new MyResult<object>(-1, "身份证号不能为空！");
//             }
//             var isV = DataValidUtil.IsIDCard(model.CertNo);
//             if (!isV)
//             {
//                 return new MyResult<object>(-1, "身份证号不合法");
//             }
//             if (!DataValidUtil.IsEMail(model.Alipay) && !DataValidUtil.IsMobile(model.Alipay))
//             {
//                 return new MyResult<object>(-1, "请求输入正常的支付宝号");
//             }
//             #endregion

//             MyResult<object> VerfiyRult = await YoyoUserSerivce.IsFaceAuth(new AuthenticationDto()
//             {
//                 TrueName = model.CertName,
//                 IdNum = model.CertNo,
//                 Alipay = model.Alipay
//             }, base.TokenModel.Id);
//             if (VerfiyRult != null) { return VerfiyRult; }

//             #region 防重复操作
//             var info = MemoryCacheUtil.Get($"FaceInit{model.CertNo}");
//             if (info == null)
//             {
//                 MemoryCacheUtil.Set($"FaceInit{model.CertNo}", true, 1);
//             }
//             else
//             {
//                 return new MyResult<object>(-1, "点击频繁 稍后再试");
//             }
//             #endregion

//             try
//             {
//                 RspRealVerifyInitiate rult = await RealVerify.Execute(new ReqRealVerifyInitiate()
//                 {
//                     SceneId = "1000000095",
//                     BizCode = "FACE_SDK",
//                     OuterOrderNo = Gen.NewGuid(),
//                     CertNo = model.CertNo.ToUpper(),
//                     CertName = model.CertName
//                 });

//                 if (rult.IsError)
//                 {
//                     result.Code = -1;
//                     result.Message = rult.ErrMsg;
//                     return result;
//                 }
//                 result.Data = new FaceModel()
//                 {
//                     CertifyId = rult.Data.CertifyId,
//                     CertifyUrl = rult.Data.CertifyUrl
//                 };
//                 await YoyoUserSerivce.WriteInitRecord(new AuthenticationDto()
//                 {
//                     AuthType = 0,
//                     CertifyId = rult.Data.CertifyId,
//                     CharacterUrl = rult.Data.CertifyUrl,
//                     TrueName = model.CertName,
//                     IdNum = model.CertNo.ToUpper(),
//                     Alipay = model.Alipay
//                 });
//                 //RedisCache.Set("CertFace:" + rult.Data.CertifyId, model.CertNo, 14400);
//                 //var response = await YoyoUserSerivce.ScanFaceInit(new AuthenticationDto()
//                 //{
//                 //    AuthType = 0,
//                 //    CertifyId = rult.Data.CertifyId,
//                 //    TrueName = model.CertName,
//                 //    IdNum = model.CertNo
//                 //}, base.TokenModel.Id);
//                 //if (response.Code != 0)
//                 //{
//                 //    return response;
//                 //}
//                 return result;
//             }
//             catch (System.Exception ex)
//             {
//                 LogUtil<ApiV2Controller>.Error(ex.Message);
//                 return new MyResult<object>(-1, "系统错误 请联系管理员");
//             }
//         }
//     }
// }