﻿using application.services.bases;
using Dapper;
using domain.configs;
using domain.repository;
using infrastructure.utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace application.services
{
    public class UserAddress : BaseServiceLfex, IUserAddress
    {
        public UserAddress(IOptionsMonitor<ConnectionStringList> connectionStringList) : base(connectionStringList)
        {

        }
        /// <summary>
        /// 获取地址列表
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<MyResult<List<domain.models.yoyoDto.UserAddress>>> AddressList(int userId)
        {
            List<domain.models.yoyoDto.UserAddress> list = new List<domain.models.yoyoDto.UserAddress>();
            MyResult<List<domain.models.yoyoDto.UserAddress>> result = new MyResult<List<domain.models.yoyoDto.UserAddress>>();
            try
            {
                if (userId <= 0) { return new MyResult<List<domain.models.yoyoDto.UserAddress>> { Code = -1, Message = "获取地址列表发生错误[SIGN]" }; }
                list = (await base.dbConnection.QueryAsync<domain.models.yoyoDto.UserAddress>($"SELECT Id,UserId,`Name`,Phone,Province,City,Area,Address,PostCode,IsDefault FROM `yoyo_member_address` WHERE IsDel=0 AND  UserId=@userId", new { userId })).ToList();
                result.Data = list;
            }
            catch (Exception)
            {
                result.Data = new List<domain.models.yoyoDto.UserAddress>();
            }
            return result;
        }
        /// <summary>
        /// 删除地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="AddressId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> DelAddress(int userId, int AddressId)
        {
            try
            {
                if (userId <= 0) { return new MyResult<object> { Code = -1, Message = "删除收货地址发生错误[SIGN]" }; }
                var Row = await base.dbConnection.ExecuteAsync($"UPDATE yoyo_member_address SET IsDel=1 WHERE UserID=@userId AND Id=@AddressId", new { userId, AddressId });
                if (Row == 1)
                {
                    return new MyResult<object> { Code = 200, Data = true };
                }
                return new MyResult<object> { Code = -1, Message = "删除地址发生错误[ERROR]" };
            }
            catch
            {
                return new MyResult<object> { Code = -1, Message = "删除地址发生错误[SYS]" };
            }
        }
        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="AddressId"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SetDefault(int userId, int AddressId)
        {
            try
            {
                if (userId <= 0) { return new MyResult<object> { Code = -1, Message = "设置默认收货地址发生错误[SIGN]" }; }
                await base.dbConnection.ExecuteAsync($"UPDATE yoyo_member_address SET IsDefault=0 WHERE UserID=@userId", new { userId, });
                var Row = await base.dbConnection.ExecuteAsync($"UPDATE yoyo_member_address SET IsDefault=1 WHERE UserID=@userId AND Id=@AddressId", new { userId, AddressId });
                if (Row == 1)
                {
                    return new MyResult<object> { Code = 200, Data = true };
                }
                return new MyResult<object> { Code = -1, Message = "设置默认收货地址发生错误[ERROR]" };
            }
            catch
            {
                return new MyResult<object> { Code = -1, Message = "设置默认收货地址发生错误[SYS]" };
            }
        }
        /// <summary>
        /// 设置地址
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<MyResult<object>> SetAddress(int userId, domain.models.yoyoDto.UserAddress req)
        {
            if (req == null) { return new MyResult<object> { Code = -1, Message = "地址变更发生错误[SYS]" }; }
            if (userId <= 0) { return new MyResult<object> { Code = -1, Message = "地址变更发生错误[SIGN]" }; }
            req.UserId = userId;
            try
            {
                if (String.IsNullOrWhiteSpace(req.Name)) { return new MyResult<object> { Code = -1, Message = "收件人不存在" }; }
                if (!DataValidUtil.IsMobile(req.Phone)) { return new MyResult<object> { Code = -1, Message = "请输入正确的手机号码" }; }
                if (String.IsNullOrWhiteSpace(req.Province)) { return new MyResult<object> { Code = -1, Message = "请输入正确的收货省份，直辖市直接输入城市名称" }; }
                if (String.IsNullOrWhiteSpace(req.City)) { return new MyResult<object> { Code = -1, Message = "请输入正确的收货城市名称" }; }
                if (String.IsNullOrWhiteSpace(req.Area)) { return new MyResult<object> { Code = -1, Message = "请输入正确的收货区县名称" }; }
                if (String.IsNullOrWhiteSpace(req.Address)) { return new MyResult<object> { Code = -1, Message = "请输入正确的收货地址" }; }
                if (String.IsNullOrWhiteSpace(req.PostCode)) { return new MyResult<object> { Code = -1, Message = "请输入正确的邮政编码" }; }
                if (!Regex.IsMatch(req.PostCode, @"^\d{6}$")) { return new MyResult<object> { Code = -1, Message = "请输入正确的邮政编码" }; }
                int ChangeRow = 0;
                if (req.Id <= 0)
                {
                    req.IsDefault = 0;
                    ChangeRow = await base.dbConnection.ExecuteAsync("INSERT INTO `yoyo_member_address` (`UserId`, `Name`, `Phone`, `Province`, `City`, `Area`, `Address`, `PostCode`, `IsDefault`, `IsDel`) VALUES (@UserId,@Name, @Phone, @Province, @City, @Area, @Address,@PostCode,0, 0)", req);
                }
                else
                {
                    ChangeRow = await base.dbConnection.ExecuteAsync("UPDATE `yoyo_member_address` SET `Name`=@Name, `Phone`=@Phone, `Province`=@Province, `City`=@City, `Area`=@Area, `Address`=@Address, `PostCode`=@PostCode WHERE `Id`=@Id AND UserId=@UserId", req);
                }
                if (ChangeRow == 1)
                {
                    return new MyResult<object> { Code = 200, Data = true };
                }
                else
                {
                    return new MyResult<object> { Code = -1, Message = (req.Id > 0 ? "修改" : "添加") + "收货地址发生错误" };
                }
            }
            catch (Exception)
            {
                return new MyResult<object> { Code = -1, Message = "地址变更发生错误[SYS]" };
            }
        }
    }
}
