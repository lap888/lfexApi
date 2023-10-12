using System.Data;
using domain.configs;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace application.services.bases
{
    public class BaseServiceLfex : DbRepository<domain.lfexentitys.lfex_serviceContext>
    {
        protected readonly IDbConnection dbConnection;
        public ConnectionStringList ConnectionStringList { get; set;}

        public BaseServiceLfex(IOptionsMonitor<ConnectionStringList> monitor)
        {
            ConnectionStringList = monitor.CurrentValue;
            if (dbConnection == null)
            {
                dbConnection = new MySqlConnection(ConnectionStringList.yoyoServiceConStr);

            }
        }
        public void Dispose()
        {
            dbConnection.Close();
        }
    }
}