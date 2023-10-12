using System;
using System.Collections.Generic;
using System.Text;

namespace domain.enums
{
    /// <summary>
    /// 步骤类型
    /// </summary>
    public enum StepType
    {
        /// <summary>
        /// 网址
        /// </summary>
        Url = 1,

        /// <summary>
        /// 图文
        /// </summary>
        Graphic = 2,

        /// <summary>
        /// 二维码
        /// </summary>
        QRCode = 3,

        /// <summary>
        /// 复制数据
        /// </summary>
        CopyData = 4,

        /// <summary>
        /// 收集截图
        /// </summary>
        Screenshots = 5,

        /// <summary>
        /// 收集信息
        /// </summary>
        Info = 6
    }
}
