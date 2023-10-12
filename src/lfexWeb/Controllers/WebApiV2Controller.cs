using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using application;
using application.services;
using domain.configs;
using domain.entitys;
using domain.enums;
using domain.models;
using domain.models.dto;
using domain.models.yoyoDto;
using domain.repository;
using infrastructure.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webAdmin.Controllers.Base;

namespace webAdmin.Controllers
{
    [Route("webapiV2/[action]")]
    public class WebApiV2Controller : WebBaseController
    {

        public IYoyoWebService YoyoWebService { get; set; }
        private readonly IQCloudPlugin QCloudSub;
        public WebApiV2Controller(IYoyoWebService yoyoWebService, IQCloudPlugin qCloud)
        {
            YoyoWebService = yoyoWebService;
            QCloudSub = qCloud;
        }
        [HttpPost]
        public MyResult<object> BannerList([FromBody]BannerDto model)
        {
            return YoyoWebService.BannerList(model);
        }
        [HttpPost]
        public MyResult<object> DelBanner([FromBody]BannerDto model)
        {
            return YoyoWebService.DelBanner(model);
        }

        [HttpPost]
        public async Task<MyResult<object>> BannerAdd_Updata([FromBody] BannerDto model)
        {
            if (!string.IsNullOrEmpty(model.ImageUrl) && model.ImageUrl.Length > 1000)
            {
                try
                {
                    String BasePic = model.ImageUrl;
                    var fileName = DateTime.Now.GetTicket().ToString();
                    String FilePath = PathUtil.Combine("Web_Game_Path", SecurityUtil.MD5(fileName).ToLower() + ".png");
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
                    model.ImageUrl = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    LogUtil<WebApiV2Controller>.Debug(ex, ex.Message);
                    return new MyResult<object>() { Code = -1, Message = "banner上传失败" };
                }
            }
            return YoyoWebService.AddBanner(model);
        }
        #region 消息
        [HttpPost]
        public MyResult<object> NoticeList([FromBody]NoticeDto model)
        {
            return YoyoWebService.NoticeList(model);
        }
        [HttpPost]
        public MyResult<object> DelNotice([FromBody]NoticeDto model)
        {
            return YoyoWebService.DelNotice(model);
        }

        [HttpPost]
        public MyResult<object> AddNotice([FromBody] NoticeDto model)
        {
            return YoyoWebService.AddNotice(model);
        }

        #endregion


        #region 消息
        [HttpPost]
        public MyResult<object> GameList([FromBody]GameDto model)
        {
            return YoyoWebService.GameList(model);
        }
        [HttpPost]
        public MyResult<object> DelGame([FromBody]GameDto model)
        {
            return YoyoWebService.DelGame(model);
        }

        [HttpPost]
        public async Task<MyResult<object>> AddGame([FromBody] GameDto model)
        {
            if (!string.IsNullOrEmpty(model.ImageUrl) && model.ImageUrl.Length > 1000)
            {
                try
                {
                    String BasePic = model.ImageUrl;
                    var fileName = DateTime.Now.GetTicket().ToString();
                    String FilePath = PathUtil.Combine("Web_Game_Path", SecurityUtil.MD5(fileName).ToLower() + ".png");
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
                    model.ImageUrl = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    LogUtil<WebApiV2Controller>.Debug(ex, ex.Message);
                    return new MyResult<object>() { Code = -1, Message = "游戏上传失败" };
                }
                // var fileName = DateTime.Now.GetTicket().ToString();
                // model.ImageUrl = ImageHandlerUtil.SaveBase64Image(model.ImageUrl, $"{fileName}.png", Constants.Game_PATH);
            }
            return YoyoWebService.AddGame(model);
        }

        [HttpPost]
        public async Task<MyResult<object>> AddGameDetail([FromBody] GameDto model)
        {
            if (!string.IsNullOrEmpty(model.ImageUrl) && model.ImageUrl.Length > 1000)
            {
                try
                {
                    String BasePic = model.ImageUrl;
                    var fileName = DateTime.Now.GetTicket().ToString();
                    String FilePath = PathUtil.Combine("Web_Game_Path", SecurityUtil.MD5(fileName).ToLower() + ".png");
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
                    model.ImageUrl = FilePath + "?v" + DateTime.Now.ToString("MMddHHmmss");
                }
                catch (Exception ex)
                {
                    LogUtil<WebApiV2Controller>.Debug(ex, ex.Message);
                    return new MyResult<object>() { Code = -1, Message = "游戏详情图上传失败" };
                }
                // var fileName = DateTime.Now.GetTicket().ToString();
                // model.ImageUrl = ImageHandlerUtil.SaveBase64Image(model.ImageUrl, $"{fileName}.png", Constants.Game_PATH);
            }
            return YoyoWebService.AddGameDetail(model);
        }

        [HttpGet]
        public MyResult<object> GameDetailList(int id)
        {
            return YoyoWebService.GameDetailList(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public MyResult<object> AuthList([FromBody]AuthDto model)
        {
            return YoyoWebService.AuthList(model);
        }

        [HttpPost]
        public MyResult<object> AgreeAuth([FromBody]AuthDto model)
        {
            return YoyoWebService.AgreeAuth(model);
        }

        [HttpPost]
        public MyResult<object> DeviceList([FromBody]LoginHistoryDto model)
        {
            return YoyoWebService.DeviceList(model);
        }

        [HttpPost]
        public MyResult<object> DelDevice([FromBody]LoginHistoryDto model)
        {
            return YoyoWebService.DelDevice(model);
        }
        [HttpPost]
        public MyResult<object> OrderGameList([FromBody]OrderGameDto model)
        {
            return YoyoWebService.OrderGameList(model);
        }

        [HttpPost]
        public async Task<MyResult<object>> RefreshOrderGame([FromBody]OrderGameDto model)
        {
            return await YoyoWebService.RefreshOrderGame(model);
        }
        #endregion





    }
}