using System.Collections.Generic;

namespace domain.configs
{
    public class Constants
    {
        #region KEY
        public const string YybKey = "6c96f46baf4d4383ba75873e12bd90f5";
        public const string LittleFishKey = "193ca5cb9b06405190eb81e91c81c422";
        #endregion

        #region SDW参数
        /// <summary>
        /// sdk key
        /// </summary>
        public const string SdwKey = "0f68f74ef06946edad8f8f3dd1bce9c7";
        public const string SdwKeyYQ = "9c839dc797024c18b434b533f0c127da";

        /// <summary>
        /// sdw channel
        /// </summary>
        public const string SdkChannel = "12872";

        #endregion
        /// <summary>
        /// WxAppID
        /// </summary>
        public const string WxAppID = "wx7b42c1ef46624de5";
        /// <summary>
        /// WxAppSecret
        /// </summary>
        public const string WxAppSecret = "78fe427222c48b49d205ecca99f4ab0c";
        /// <summary>
        /// jpushkey
        /// </summary>
        public const string JpushKey = "e316eadc22316109e388df48";

        /// <summary>
        /// jpushSecret
        /// </summary>
        public const string JpushSecret = "d1da0f32e1d1d1a0e38f835d";

        #region 默认头像
        /// <summary>
        /// 默认头像图片相对服务器路径
        /// </summary>
        public const string DefaultHeadPicture = "/images/head.png";
        #endregion

        /// <summary>
        /// 腾讯云COS 访问地址
        /// </summary>
        public const string CosUrl = "https://file.yoyoba.cn/";

        /// <summary>
        /// sign token key
        /// </summary>
        public static string Key = "xingchenwuxian";

        /// <summary>
        /// sign token key
        /// </summary>
        public static string YoyoKey = "fa94a2d8360041acb5f102c10b2efbf0";

        /// <summary>
        /// 公司明
        /// </summary>
        public static string Company = "小鱼(LF)全球区块链交易所";
        /// <summary>
        /// 网站授权协议
        /// </summary>
        public const string WEBSITE_AUTHENTICATION_SCHEME = "Web";
        /// <summary>
        /// 上次登录路径
        /// </summary>
        public const string LAST_LOGIN_PATH = "LAST_LOGIN_PATH";

        public const string ShowAllDataCookie = "ShowAllData";
        /// <summary>
        /// 验证码图片
        /// </summary>
        public const string WEBSITE_VERIFICATION_CODE = "ValidateCode";

        /// <summary>
        /// 
        /// </summary>
        public const string UPLOAD_TEMP_PATH = "Upload_Temp";
        public const string BANNER_PATH = "Banner";
        public const string Game_PATH = "Game";

        public const string TRADE_PATH = "Trade";
        //申诉截图路径
        public const string APPEALS_PATH = "Appeals";
        public const string USER_PIC = "UserPic";
        public const string MessageType_PATH = "MessageType";
        public const string Message_Path = "Message";
        public const string Shop_Logo_Path = "ShopLogo";
        public const string Shop_Detail_Path = "ShopDetail";
        public const string SCENIC_PATH = "scenic";
        public const string WxAppId = "wxffeedd2ae4c6df3a";
        public const string WxSecret = "33cb587c96804bbacebbde7fdd6a03f9";
        public const string WxPic = "WxPic";
        /// <summary>
        /// access_token
        /// </summary>
        public const string WxAccessToken = "access_token";
        public static List<Tasks> MinningListSetting = new List<Tasks>
        {
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=0,
                MinningName="小鱼云矿机",
                Pow=0.1M,
                CandyIn=0,
                CandyOut=60,
                RunTime="60天",

                DayCandyOut=1,
                MaxHave=1,
                Colors="#1e93f6"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=false,
                MinningId=1,
                Pow=0.2M,
                MinningName="节点矿机",
                CandyIn=0,
                CandyOut=60,
                RunTime="60天",

                DayCandyOut=1,
                MaxHave=1,
                Colors="#4cc7ab"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=2,
                Pow=0.4M,
                MinningName="初级矿机",
                CandyIn=10,
                CandyOut=12,
                RunTime="30天",

                DayCandyOut=0.4M,
                MaxHave=8,
                Colors="#fd2701"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=3,
                Pow=4M,
                MinningName="中级矿机",
                CandyIn=100,
                CandyOut=120,
                RunTime="30天",
                DayCandyOut=4,
                MaxHave=4,
                Colors="#FFA500"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=4,
                Pow=17.5M,
                MinningName="进阶矿机",
                CandyIn=400,
                CandyOut=525,
                RunTime="30天",

                DayCandyOut=17.5M,
                MaxHave=1,
                Colors="#4cc7ab"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=5,
                Pow=45M,
                MinningName="高级矿机",
                CandyIn=1000,
                CandyOut=1350,
                RunTime="30天",
                DayCandyOut=45,
                MaxHave=2,
                Colors="#127f07"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=6,
                Pow=182M,
                MinningName="精英矿机",
                CandyIn=4000,
                CandyOut=5460,
                RunTime="30天",
                DayCandyOut=1,
                MaxHave=1,
                Colors="#00BFFF"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=7,
                Pow=470M,
                MinningName="超级矿机",
                CandyIn=10000,
                CandyOut=14100,
                RunTime="30天",
                DayCandyOut=1,
                MaxHave=1,
                Colors="#994dd7"
            }
        };
        public static List<Tasks> TaskListSetting = new List<Tasks>
        {
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=0,
                MinningName="体验任务",
                CandyIn=0,
                CandyOut=60,
                RunTime="60天",

                DayCandyOut=1,
                MaxHave=1,
                Colors="#fd2701"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=1,
                MinningName="初级任务",
                CandyIn=60,
                CandyOut=69,
                RunTime="30天",
                CandyH=2.3M,
                TeamH=2.3M,
                CandyP=30,
                DayCandyOut=2.3M,
                MaxHave=5,
                Colors="#4cc7ab"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=2,
                MinningName="中级任务",
                CandyIn=600,
                CandyOut=720,
                RunTime="30天",
                CandyH=24M,
                TeamH=24M,
                CandyP=300,
                DayCandyOut=24,
                MaxHave=3,
                Colors="#1e93f6"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=3,
                MinningName="高级任务",
                CandyIn=1800,
                CandyOut=2250,
                RunTime="30天",
                CandyH=75M,
                TeamH=75M,
                CandyP=900,
                DayCandyOut=75,
                MaxHave=2,
                Colors="#127f07"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=4,
                MinningName="超级任务",
                CandyIn=10000,
                CandyOut=13800,
                RunTime="30天",
                CandyH=460M,
                TeamH=460M,
                CandyP=5000,
                DayCandyOut=460,
                MaxHave=1,
                Colors="#994dd7"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=5,
                MinningName="高阶任务包",
                CandyIn=4000,
                CandyOut=5460,
                RunTime="30天",
                CandyH=182M,
                TeamH=182M,
                CandyP=2000,
                DayCandyOut=182,
                MaxHave=1,
                Colors="#00BFFF"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=6,
                MinningName="一期减产体验任务",
                CandyIn=0,
                CandyOut=30,
                RunTime="60天",
                CandyH=0.5M,
                TeamH=0.5M,
                CandyP=0,
                DayCandyOut=0.5M,
                MaxHave=1,
                Colors="#fd2701"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=7,
                MinningName="初阶任务包",
                CandyIn=30,
                CandyOut=33,
                RunTime="30天",
                CandyH=1.1M,
                TeamH=1.1M,
                CandyP=15,
                DayCandyOut=1.1M,
                MaxHave=5,
                Colors="#FFA500",
                Remark="任务说明：任务包为3月15日后注册会员兑换"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=8,
                MinningName="特级任务",
                CandyIn=30000,
                CandyOut=42000,
                RunTime="30天",
                CandyH=1400M,
                TeamH=1400M,
                CandyP=15000,
                DayCandyOut= 1400M,
                MaxHave=1,
                Colors="#A0522D"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=9,
                MinningName="超特级任务",
                CandyIn=100000,
                CandyOut=141990,
                RunTime="30天",
                CandyH=4733M,
                TeamH=4733M,
                CandyP=50000,
                DayCandyOut= 4733M,
                MaxHave=1,
                Colors="#008B8B"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=10,
                MinningName="中阶任务包",
                CandyIn=300,
                CandyOut=348,
                RunTime="30天",
                CandyH=11.6M,
                TeamH=11.6M,
                CandyP=150,
                DayCandyOut= 11.6M,
                MaxHave=3,
                Colors="#4169E1"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=11,
                MinningName="二级卓越贡献任务包",
                CandyIn=0,
                CandyOut=360,
                RunTime="180天",
                CandyH=2M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 2M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=12,
                MinningName="三级卓越贡献任务包",
                CandyIn=0,
                CandyOut=900,
                RunTime="180天",
                CandyH=5M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 5M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=13,
                MinningName="四级卓越贡献任务包",
                CandyIn=0,
                CandyOut=2700,
                RunTime="180天",
                CandyH=15M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 15M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=14,
                MinningName="五级卓越贡献任务包",
                CandyIn=0,
                CandyOut=8100,
                RunTime="180天",
                CandyH=45M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 45M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=15,
                MinningName="荣耀任务包",
                CandyIn=0,
                CandyOut=36,
                RunTime="180天",
                CandyH=0.2M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 0.2M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=16,
                MinningName="体验任务",
                CandyIn=0,
                CandyOut=12,
                RunTime="30天",
                CandyH=0.4M,
                TeamH=0.4M,
                CandyP=0,
                DayCandyOut= 0.4M,
                MaxHave=1,
                Colors="#fd2701"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=17,
                MinningName="新人入门任务",
                CandyIn=10,
                CandyOut=12,
                RunTime="30天",
                CandyH=0.4M,
                TeamH=0.4M,
                CandyP=5,
                DayCandyOut= 0.4M,
                MaxHave=5,
                Colors="#DC143C",
                Remark="任务说明：任务包为5月1日后注册的新用户兑换"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=18,
                MinningName="哟哟吧专享任务",
                CandyIn=0,
                CandyOut=900,
                RunTime="90天",
                CandyH=10M,
                TeamH=0M,
                CandyP=100,
                DayCandyOut= 10M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=19,
                MinningName="参会专享任务",
                CandyIn=0,
                CandyOut=730,
                RunTime="365天",
                CandyH=2M,
                TeamH=0M,
                CandyP=0,
                DayCandyOut= 2M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=true,
                MinningId=20,
                MinningName="新人体验任务(C型)",
                CandyIn=0,
                CandyOut=60,
                RunTime="60天",
                CandyH=1M,
                TeamH=6M,
                CandyP=0,
                DayCandyOut= 1M,
                MaxHave=1,
                Colors="#fd2701"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=21,
                MinningName="客服专属任务(B型)",
                CandyIn=0,
                CandyOut=498M,
                RunTime="50天",
                CandyH=9.96M,
                TeamH=40M,
                CandyP=200,
                DayCandyOut=9.96M,
                MaxHave=1,
                Colors="#000000"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=50,
                MinningName="初级任务(股权A型)",
                CandyIn=10,
                CandyOut=12,
                RunTime="30天",
                CandyH=0.4M,
                TeamH=0.4M,
                CandyP=5,
                DayCandyOut= 0.4M,
                MaxHave=10,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=51,
                MinningName="中级任务(股权A型)",
                CandyIn=100,
                CandyOut=123,
                RunTime="30天",
                CandyH=4.1M,
                TeamH=4.1M,
                CandyP=50,
                DayCandyOut=4.1M,
                MaxHave=4,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=52,
                MinningName="中阶任务(股权A型)",
                CandyIn=400,
                CandyOut=498,
                RunTime="30天",
                CandyH=16.6M,
                TeamH=16.6M,
                CandyP=200,
                DayCandyOut=16.6M,
                MaxHave=1,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=53,
                MinningName="高级任务(股权A型)",
                CandyIn=1000,
                CandyOut=1260,
                RunTime="30天",
                CandyH=42M,
                TeamH=42M,
                CandyP=500,
                DayCandyOut=42M,
                MaxHave=2,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=54,
                MinningName="高阶任务(股权A型)",
                CandyIn=4000,
                CandyOut=4980,
                RunTime="30天",
                CandyH=166M,
                TeamH=166M,
                CandyP=2000,
                DayCandyOut=166M,
                MaxHave=1,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=55,
                MinningName="超级任务(股权A型)",
                CandyIn=10000,
                CandyOut=13800,
                RunTime="30天",
                CandyH=460M,
                TeamH=460M,
                CandyP=5000,
                DayCandyOut=460M,
                MaxHave=1,
                Colors="#cd7f32"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=56,
                MinningName="超特级任务(股权A型)",
                CandyIn=100000,
                CandyOut=141990,
                RunTime="30天",
                CandyH=4733M,
                TeamH=4733M,
                CandyP=50000,
                DayCandyOut= 4733M,
                MaxHave=1,
                Colors="#008B8B"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=57,
                MinningName="特级任务(股权A型)",
                CandyIn=30000,
                CandyOut=42000,
                RunTime="30天",
                CandyH=1400M,
                TeamH=1400M,
                CandyP=15000,
                DayCandyOut= 1400M,
                MaxHave=1,
                Colors="#008B8B"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=100,
                MinningName="初级任务(A型)",
                CandyIn=10,
                CandyOut=12,
                RunTime="30天",
                CandyH=0.4M,
                TeamH=0.4M,
                CandyP=5,
                DayCandyOut= 0.4M,
                MaxHave=10,
                Colors="#DC143C"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=101,
                MinningName="中级任务(A型)",
                CandyIn=100,
                CandyOut=123,
                RunTime="30天",
                CandyH=4.1M,
                TeamH=4.1M,
                CandyP=50,
                DayCandyOut=4.1M,
                MaxHave=4,
                Colors="#1e93f6"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=102,
                MinningName="中阶任务(A型)",
                CandyIn=400,
                CandyOut=498,
                RunTime="30天",
                CandyH=16.6M,
                TeamH=16.6M,
                CandyP=200,
                DayCandyOut=16.6M,
                MaxHave=1,
                Colors="#4169E1"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=103,
                MinningName="高级任务(A型)",
                CandyIn=1000,
                CandyOut=1260,
                RunTime="30天",
                CandyH=42M,
                TeamH=42M,
                CandyP=500,
                DayCandyOut=42M,
                MaxHave=2,
                Colors="#127f07"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=104,
                MinningName="高阶任务(A型)",
                CandyIn=4000,
                CandyOut=4980,
                RunTime="30天",
                CandyH=166M,
                TeamH=166M,
                CandyP=2000,
                DayCandyOut=166M,
                MaxHave=1,
                Colors="#00BFFF"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=105,
                MinningName="超级任务(A型)",
                CandyIn=10000,
                CandyOut=13800,
                RunTime="30天",
                CandyH=460M,
                TeamH=460M,
                CandyP=5000,
                DayCandyOut=460M,
                MaxHave=1,
                Colors="#994dd7"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=106,
                MinningName="超特级任务(A型)",
                CandyIn=100000,
                CandyOut=141990,
                RunTime="30天",
                CandyH=4733M,
                TeamH=4733M,
                CandyP=50000,
                DayCandyOut= 4733M,
                MaxHave=1,
                Colors="#008B8B"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=110,
                MinningName="初级任务(B型)",
                CandyIn=10,
                CandyOut=12M,
                RunTime="50天",
                CandyH=0.24M,
                TeamH=1M,
                CandyP=5,
                DayCandyOut= 0.24M,
                MaxHave=10,
                Colors="#DC143C"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=111,
                MinningName="中级任务(B型)",
                CandyIn=100,
                CandyOut=123M,
                RunTime="50天",
                CandyH=2.46M,
                TeamH=10M,
                CandyP=50,
                DayCandyOut=2.46M,
                MaxHave=4,
                Colors="#1e93f6"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=112,
                MinningName="中阶任务(B型)",
                CandyIn=400,
                CandyOut=498M,
                RunTime="50天",
                CandyH=9.96M,
                TeamH=40M,
                CandyP=200,
                DayCandyOut=9.96M,
                MaxHave=1,
                Colors="#4169E1"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=113,
                MinningName="高级任务(B型)",
                CandyIn=1000,
                CandyOut=1260,
                RunTime="50天",
                CandyH=25.2M,
                TeamH=100M,
                CandyP=500,
                DayCandyOut=25.2M,
                MaxHave=2,
                Colors="#127f07"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=114,
                MinningName="高阶任务(B型)",
                CandyIn=4000,
                CandyOut=4980M,
                RunTime="50天",
                CandyH=99.6M,
                TeamH=400M,
                CandyP=2000,
                DayCandyOut=99.6M,
                MaxHave=1,
                Colors="#00BFFF"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=true,
                StoreShow=true,
                MinningId=115,
                MinningName="超级任务(B型)",
                CandyIn=10000,
                CandyOut=13800,
                RunTime="50天",
                CandyH=276M,
                TeamH=1000M,
                CandyP=5000,
                DayCandyOut=276M,
                MaxHave=1,
                Colors="#994dd7"
            },
            new Tasks{
                IsExchange=false,
                IsRenew=false,
                StoreShow=false,
                MinningId=117,
                MinningName="特级任务(B型)",
                CandyIn=30000,
                CandyOut=42000,
                RunTime="50天",
                CandyH=840M,
                TeamH=3000M,
                CandyP=15000,
                DayCandyOut=840M,
                MaxHave=1,
                Colors="#A0522D"
            },
            new Tasks{
                IsExchange=true,
                IsRenew=false,
                StoreShow=true,
                MinningId=120,
                MinningName="初级任务(C型)",
                CandyIn=60,
                CandyOut=66,
                RunTime="60天",
                CandyH=1.1M,
                TeamH=6M,
                CandyP=12,
                DayCandyOut= 1.1M,
                MaxHave=1,
                Colors="#4cc7ab"
            },
        };
    }
}