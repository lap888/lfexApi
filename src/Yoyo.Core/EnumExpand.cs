﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Yoyo.Core
{
    public static class EnumExpand
    {
        /// <summary>
        /// 获取枚举描述信息
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            var description = value.GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description;

            return String.IsNullOrWhiteSpace(description) ? value.ToString() : description;
        }
    }
}
