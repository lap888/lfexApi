﻿using application.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace application.Request
{
    /// <summary>
    /// 获取文件
    /// </summary>
    public class ReqCosGetObject : IQCloudRequest<Response.RspCosGetObject>
    {
        private String FilePath;

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="path">文件路径：/开始</param>
        public ReqCosGetObject(String path)
        {
            this.FilePath = path;
        }

        /// <summary>
        /// 请求方式
        /// </summary>
        /// <returns></returns>
        public Enums.QCloudMethod GetMethod()
        {
            return Enums.QCloudMethod.get;
        }

        /// <summary>
        /// 路径
        /// </summary>
        /// <returns></returns>
        public String GetPath()
        {
            return FilePath;
        }

        /// <summary>
        /// 请求体
        /// </summary>
        /// <returns></returns>
        public HttpContent GetContent()
        {
            return null;
        }

        /// <summary>
        /// 获取头参数
        /// </summary>
        /// <returns></returns>
        public UtilDictionary GetHeaderParam()
        {
            UtilDictionary HeaderParam = new UtilDictionary();

            return HeaderParam;
        }

        /// <summary>
        /// 请求参数
        /// </summary>
        /// <returns></returns>
        public UtilDictionary GetHttpParam()
        {
            UtilDictionary Param = new UtilDictionary();

            return Param;
        }
    }
}
