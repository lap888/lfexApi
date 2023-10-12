using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CSRedis;
using Dapper;
using domain.lfexentitys;
using domain.models.lfexDto;
using infrastructure.utils;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Xunit;

namespace test
{
    public class AddRP
    {

        [Fact]
        public async Task AddRPM()
        {
            CSRedisClient c = new CSRedisClient("129.28.186.13:6379,password=yoyoba,defaultDatabase=7,prefix=G_");
            //Given
            using (IDbConnection dbConnection = new MySqlConnection("server=bj-cdb-061ud0fa.sql.tencentcdb.com;port=61617;user id=lfex_service_r;password=Guo123456-lfex_service_r;database=lfex_service_r;Charset=utf8mb4;"))
            {
                // await AddPeople(dbConnection, 69.ToString(), c, 10);
                // AddPeople(dbConnection, 9696.ToString(), c, 10).GetAwaiter().GetResult();
                // await AddPeople(dbConnection, 888.ToString(), c, 5);
                // await AddPeople(dbConnection, 999.ToString(), c, 2);
                // await AddPeople(dbConnection, 15157957420.ToString(), c, 35);
                // await AddPeople(dbConnection, 15697932838.ToString(), c, 35);

                await AddPeople(dbConnection, 1357111.ToString(), c, 29);

                System.Console.ReadLine();
            }
        }

        [Fact]
        public void AddAuth()
        {
            CSRedisClient RedisCache = new CSRedisClient("129.28.186.13:6379,password=yoyoba,defaultDatabase=7,prefix=G_");
            //Given
            using (IDbConnection dbConnection = new MySqlConnection("server=bj-cdb-061ud0fa.sql.tencentcdb.com;port=61617;user id=lfex_service_r;password=Guo123456-lfex_service_r;database=lfex_service_r;Charset=utf8mb4;"))
            {
                int[] authIds = new int[]
                {
                    6823,
                    6824,
                    6825,
                    6826,
                    6827,
                    6828,
                    6829,
                    6830,
                    6833,
                    6834,
                    6835,
                    6836,
                    6837,
                    6838,
                    6839,
                    6840,
                    6841,
                    6842,
                    6843,
                    6844,
                    6845,
                    6846,
                    6847,
                    6848,
                    6849,
                    6850,
                    6851,
                    6852,
                    6853,
                    6854
                };
                dbConnection.Open();
                Random random = new Random();
                // int[] timeNum = new int[] { 20000, 60000, 15000, 60000, 19000, 50000, 90000, 100000, 63000, 120000, 140000 };
                int[] timeNum = new int[] { 100000 };
                for (int i = 0; i < authIds.Length; i++)
                {
                    int time = timeNum[random.Next(0, timeNum.Length - 1)];
                    Auth(dbConnection, authIds[i], RedisCache);
                    System.Console.WriteLine($"ID:{i},用户:{authIds[i]}已经认证");
                    System.Threading.Thread.Sleep(time);
                }
                dbConnection.Close();
                System.Console.WriteLine("实名认证完成...");
                System.Console.ReadLine();
            }
        }

        public async Task AddPeople(IDbConnection dbConnection, string invitationCode, CSRedisClient c, int totalAmount)
        {
            Random random = new Random();
            var inviterMobile = dbConnection.QueryFirstOrDefault<string>($"select mobile from user where mobile = '{invitationCode}' or rcode='{invitationCode}'");

            int[] timeNum = new int[] { 200000, 60000, 150000, 60000, 190000, 50000, 90000, 100000, 63000, 120000, 140000 };
            // int[] timeNum = new int[] { 20000 };
            for (int i = 0; i < totalAmount; i++)
            {
                int time = timeNum[random.Next(0, timeNum.Length - 1)];
                var num = GetRandomTel();
                var nickName = GetNames();
                var trueNiceNameCount = dbConnection.QueryFirstOrDefault<int>("select count(id) count from user");
                var databaseName = GetDataBaseNames(dbConnection, trueNiceNameCount);
                var name = databaseName + nickName;
                await SignUp(dbConnection, num, name, inviterMobile, c, i);
                System.Console.WriteLine($"==={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}===\r\nid:{i}\r\n手机号:{num}\r\n上级手机号:{inviterMobile}\r\n昵称:{name}\r\n邀请码:{invitationCode}");
                System.Threading.Thread.Sleep(time);
            }
            System.Console.WriteLine($"{invitationCode}...ok!");
        }

        public async Task<bool> SignUp(IDbConnection dbConnection, string mobile, string nickName, string invitationCode, CSRedisClient RedisCache, int i)
        {
            //查询用户手机号是否存在
            var mobileIsHave = dbConnection.QueryFirstOrDefault<int>($"select id from user where mobile='{mobile}'");
            if (mobileIsHave != 0)
            {
                System.Console.WriteLine("手机号已注册${mobile}");
                return false;
            }
            try
            {
                int insertUser = 0;
                dbConnection.Open();
                using (IDbTransaction transaction = dbConnection.BeginTransaction())
                {
                    try
                    {
                        insertUser = dbConnection.ExecuteScalar<int>($"INSERT INTO user (cCount,mobile,avatarUrl,password, passwordSalt, name, inviterMobile, uuid,tradePwd,auditState,alipay) VALUES (0,'{mobile}','lfex/images/avatar/default/1.png', 'E10ADC3949BA59ABBE56E057F20F883E', '', '{nickName}','{invitationCode}', '{Guid.NewGuid().ToString("N")}','E10ADC3949BA59ABBE56E057F20F883E',0,'{mobile}');select @@IDENTITY", null, transaction);
                        if (insertUser <= 0)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        //增加一条用户额外信息记录
                        var rows = dbConnection.Execute($"insert into `user_ext`(`userId`) values({insertUser})", null, transaction);
                        if (rows != 1)
                        {
                            transaction.Rollback();
                            return false;
                        }
                        //赠送矿机
                        var source = 0;
                        var taskId = 0;
                        var effectiveBiginTime = DateTime.Now.Date.ToLocalTime().ToString("yyyy-MM-dd");
                        var effectiveEndTime = DateTime.Now.Date.AddYears(60).ToLocalTime().ToString("yyyy-MM-dd");
                        await dbConnection.ExecuteAsync($"insert into minnings (userId, minningId, beginTime, endTime, source,minningStatus) values ({insertUser}, {taskId}, '{effectiveBiginTime}', '{effectiveEndTime}', {source},0);", null, transaction);
                        //写入币种钱包
                        var coinTypeSql = "select name,type from `coin_type` where status=0 or status=1";
                        var coinTypes = await dbConnection.QueryAsync<CoinTypeModel>(coinTypeSql, null, transaction);
                        var flag = false;
                        coinTypes.ToList().ForEach(action =>
                        {
                            rows = dbConnection.Execute("INSERT INTO user_account_wallet (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`,`Type`,`CoinType`) VALUES (@UserId, '0', '0', '0', '0', NOW(),@Type,@CoinType)", new { UserId = insertUser, Type = action.Type, CoinType = action.Name }, transaction);
                            if (rows != 1)
                            {
                                transaction.Rollback();
                                flag = true;
                            }
                        });
                        if (flag)
                        {
                            return false;
                        }

                        transaction.Commit();
                        long ParentId = dbConnection.QueryFirstOrDefault<int>($"select id from user where mobile = '{invitationCode}' or rcode='{invitationCode}'");
                        if (ParentId <= 0)
                        {
                            ParentId = 0;
                        }
                        try
                        {
                            var c = RedisCache.Publish("YoYo_Member_Regist", JsonConvert.SerializeObject(new { MemberId = insertUser, ParentId = ParentId }));
                            if (c == 0)
                            {
                                System.Console.WriteLine($"消息推送失败{mobile}");
                            }
                        }
                        catch (System.Exception)
                        {
                            System.Console.WriteLine($"发消息异常{mobile}");
                            return false;
                        }
                        if (i != 3 || i != 7 || i != 13 || i != 17)
                        {
                            //实名Auth
                            // Auth(dbConnection, insertUser, RedisCache);
                            MyLogger.WriteMessage(insertUser.ToString(), invitationCode);
                        }
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                    finally { if (dbConnection.State == ConnectionState.Open) { dbConnection.Close(); } }
                }
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"系统错误=>执行手机号={mobile}输出日志={ex}");
                return false;
            }
        }

        public void Auth(IDbConnection dbConnection, int userId, CSRedisClient RedisCache)
        {
            using (IDbTransaction transaction = dbConnection.BeginTransaction())
            {
                try
                {
                    var rowId = dbConnection.Execute($"update user set `auditState`=2,`golds`=(`golds`+50),`level`='LV1' where id = {userId} and auditState<>2", null, transaction);
                    if (rowId <= 0)
                    {
                        transaction.Rollback();
                    }
                    // 贡献值
                    dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({userId},50,'实名认证赠送50贡献值',1,now(),now())", null, transaction);

                    //邀请人
                    var user = dbConnection.QueryFirstOrDefault<User>($"select `inviterMobile`,`name` from user where id={userId}", null, transaction);
                    //写入记录
                    var inviterUser = dbConnection.QueryFirstOrDefault<User>($"select id,golds from user where mobile='{user.InviterMobile}'", null, transaction);
                    if (inviterUser != null)
                    {
                        dbConnection.Execute($"insert into `user_candyp`(`userId`,`candyP`,`content`,`source`,`createdAt`,`updatedAt`) values({inviterUser.Id},50,'下级「{user.Name}」实名认证赠送50贡献值',1,now(),now())", null, transaction);
                        //贡献值
                        var gold = (int)inviterUser.Golds + 50;
                        var level = CaculatorGolds(gold);
                        dbConnection.Execute($"update user set `golds`={gold},`level`='{level}' where `id`={inviterUser.Id}", null, transaction);
                    }
                    transaction.Commit();
                }
                catch (System.Exception ex)
                {
                    transaction.Rollback();
                    System.Console.WriteLine($"实名失败「{ex.Message}」");
                }
            }
            try
            {
                long c = RedisCache.Publish("YoYo_Member_Certified", JsonConvert.SerializeObject(new { MemberId = userId }));
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"实名失败「{ex.Message}」");
            }
        }
        private string CaculatorGolds(int golds)
        {
            var level = "lv0";
            if (golds >= 5000)
            {
                level = "lv5";
            }
            else if (golds >= 2000)
            {
                level = "lv4";
            }
            else if (golds >= 200)
            {
                level = "lv3";
            }
            else if (golds >= 100)
            {
                level = "lv2";
            }
            else if (golds >= 50)
            {
                level = "lv1";
            }
            else
            {
                level = "lv0";
            }
            return level;
        }

        #region 手机号 昵称
        private string[] telStarts = "198,199,177,173,150,151,138,137,166,131,150,187,159,153,134,135,137".Split(',');
        Random ran = new Random();
        /// <summary>
        /// 随机手机号
        /// </summary>
        /// <returns></returns>
        public string GetRandomTel()
        {
            int index = ran.Next(0, telStarts.Length - 1);
            string first = telStarts[index];
            string second = (ran.Next(100, 888) + 10000).ToString().Substring(1);
            string thrid = (ran.Next(1, 9100) + 10000).ToString().Substring(1);
            return first + second + thrid;
        }


        public static string GetDataBaseNames(IDbConnection dbConnection, int count)
        {
            Random ran = new Random();
            var randomNum = ran.Next(1, count);
            var name = dbConnection.QueryFirstOrDefault<string>($"select name from user where id={randomNum}");
            return name;
        }
        /// <summary>
        /// 随机生成昵称
        /// </summary>
        /// <returns></returns>
        public static string GetNames()
        {

            Random ran = new Random();
            string[] nameS1 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "u", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] nameS2 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] nameS3 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] nameS4 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] nameS5 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] nameS6 = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };



            string s1 = nameS1[ran.Next(0, nameS1.Length - 1)];
            string s2 = nameS2[ran.Next(0, nameS2.Length - 1)];
            string s3 = nameS3[ran.Next(0, nameS3.Length - 1)];
            string s4 = nameS4[ran.Next(0, nameS4.Length - 1)];
            string s5 = nameS5[ran.Next(0, nameS5.Length - 1)];
            string s6 = nameS6[ran.Next(0, nameS6.Length - 1)];

            string[] len1 = new string[] { "1", "1" };
            string s7 = len1[ran.Next(0, len1.Length - 1)];
            string name = string.Empty;
            if (s7 == "1")
            {
                name = s1;
            }
            else if (s7 == "2")
            {
                name = s1 + s2;
            }
            else if (s7 == "3")
            {
                name = s1 + s2 + s3;
            }
            else if (s7 == "4")
            {
                name = s1 + s2 + s3 + s4;
            }
            else if (s7 == "5")
            {
                name = s1 + s2 + s3 + s4 + s5;
            }
            else if (s7 == "6")
            {
                name = s1 + s2 + s3 + s4 + s5 + s6;
            }
            else
            {
                name = string.Empty;
            }
            return name;
        }
        #endregion

    }
}
