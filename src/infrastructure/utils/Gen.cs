using System;

namespace infrastructure.utils
{
    public static class Gen
    {
        /// <summary>
        /// 生成32位 yyyyMMddHHmmssffffff+hashcode
        /// </summary>
        /// <returns></returns>
        public static string NewGuid()
        {

            var orderdate = DateTime.Now.ToString("yyyyMMddHHmmssffffff");
            var ordercode = Guid.NewGuid().GetHashCode();
            var num = 32 - orderdate.Length;
            if (ordercode < 0) { ordercode = -ordercode; }
            var orderlast = ordercode.ToString().Length > num ? ordercode.ToString().Substring(0, num) : ordercode.ToString().PadLeft(num, '0');
            return $"{orderdate}{orderlast}";
        }
        /// <summary>
        /// 20位数 yyyyMMddHHmmss+hashcode
        /// </summary>
        /// <returns></returns>
        public static string NewGuid20()
        {
            var orderdate = DateTime.Now.ToString("yyyyMMddHHmmss");
            var ordercode = Guid.NewGuid().GetHashCode();
            var num = 20 - orderdate.Length;
            if (ordercode < 0) { ordercode = -ordercode; }
            var orderlast = ordercode.ToString().Length > num ? ordercode.ToString().Substring(0, num) : ordercode.ToString().PadLeft(num, '0');
            return $"{orderdate}{orderlast}";
        }

        public static string NewGuidN(string str = "yb")
        {
            var orderdate = Guid.NewGuid().ToString("N");
            return $"{str}{orderdate}";
        }
    }
}