using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using domain.repository;
using infrastructure.extensions;
using infrastructure.utils;
using Xunit;

namespace test
{
    public class HttpReqs
    {
        [Fact]
        public async Task Reqs()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    GenSign();
                    var requestUri = "https://dev.yoyoba.cn/api/System/Banners?source=0";
                    var httpResponseMessage = await httpClient.GetAsync(requestUri);
                    //do something...
                    string res = await httpResponseMessage.Content.ReadAsStringAsync();
                    var httpReq = HttpUtil.GetString(requestUri).GetModel();

                    System.Console.WriteLine(httpResponseMessage);
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine(ex);
                throw;
            }
        }

        public MyResult<object> GenSign()
        {
            MyResult result = new MyResult();
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>();
            DateTimeOffset dto = new DateTimeOffset(DateTime.Now);
            var timeSpan = dto.ToUnixTimeSeconds().ToString().Substring(0, 10);

            var sign = string.Empty;
            //f=iphone
            //&is_day_activation=1
            //&mobile=18333103619
            //&mobile_code=102956
            //&mobile_code_check_type=msg
            //&sign=C6BAAAA78D9D88D5C11BBD1269020DDF
            //&time=1605886365
            //&v=9.9.0&weixin=1
            // dict.Add("nick", WebUtility.UrlEncode(""));

            dict.Add("f", "iphone");
            dict.Add("is_day_activation", "1");
            dict.Add("mobile", "18333103619");
            dict.Add("mobile_code", "102956");
            dict.Add("mobile_code_check_type", "msg");
            dict.Add("time", "1605886365");
            dict.Add("v", "9.9.0");
            dict.Add("weixin", "1");



            var content = string.Join("&", dict.Where(t => !string.IsNullOrEmpty(t.Value))
                  .Select(t => $"{t.Key}={t.Value}")).OrderBy(t => t).ToString();
            sign = SecurityUtil.MD5(content);

            //   var list = dict.Select(t => t.ToUpper()).OrderBy(t => t).Where(t => !string.IsNullOrEmpty(t)).ToArray();
            result.Data = sign;

            return result;
        }
    }
}