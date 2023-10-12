using System;
using System.Collections.Generic;
using System.Text;

namespace domain.models
{
    /// <summary>
    /// 手机号信息
    /// </summary>
    public class PhoneInfoModel
    {
        /// <summary>
        /// 运营商名称
        /// </summary>
        public String Operator { get; set; }

        /// <summary>
        /// 城市编码
        /// </summary>
        public String CityCode { get; set; }

        /// <summary>
        /// 可充值面值
        /// </summary>
        public List<FaceValueModel> FaceValue { get; set; }

        /// <summary>
        /// 城市名称
        /// </summary>
        public String City { get; set; }

        /// <summary>
        /// 省份名称
        /// </summary>
        public String Province { get; set; }

        /// <summary>
        /// 运营商类型
        /// </summary>
        public String SpType { get; set; }
    }
}

/// <summary>
/// 充值面值
/// </summary>
public class FaceValueModel
{
    /// <summary>
    /// 面值
    /// </summary>
    public String FaceValue { get; set; }

    /// <summary>
    /// 糖果数
    /// </summary>
    public Decimal CandyNum { get; set; }

    /// <summary>
    /// 手收费
    /// </summary>
    public Decimal Fee { get; set; }
}
