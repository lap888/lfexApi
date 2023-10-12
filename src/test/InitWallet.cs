using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using domain.models.lfexDto;
using MySql.Data.MySqlClient;
using Xunit;

namespace test
{
    public class UnitTest1
    {
        
        [Fact]
        public void initwallet()
        {
            using (IDbConnection dbConnection = new MySqlConnection("server=129.28.186.13;port=3306;user id=yoyoba;password=Yoyo123...;database=lfex_service;Charset=utf8mb4;"))
            {

                //所有用户
                var userIds = dbConnection.Query<int>($"select id from user;");
                var coinTypeSql = "select name,type from `coin_type` where status=0 or status=1";
                var coinTypes = dbConnection.Query<CoinTypeModel>(coinTypeSql);
                userIds.ToList().ForEach(userId =>
                {
                    //写入币种钱包
                    coinTypes.ToList().ForEach(action =>
                    {
                        var ishavecoin = dbConnection.QueryFirstOrDefault($"select * from user_account_wallet where CoinType='{action.Name}' and UserId={userId}");
                        if (ishavecoin == null)
                        {
                            var rows = dbConnection.Execute("INSERT INTO user_account_wallet (`UserId`, `Revenue`, `Expenses`, `Balance`, `Frozen`, `ModifyTime`,`Type`,`CoinType`) VALUES (@UserId, '0', '0', '0', '0', NOW(),@Type,@CoinType)", new { UserId = userId, Type = action.Type, CoinType = action.Name });
                            System.Console.WriteLine($"用户{userId},补建钱包币种{action.Name}");
                        }
                    });

                });
                System.Console.WriteLine($"ok...");
            }
        }
    }
}
