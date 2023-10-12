// using application.services.bases;
// using domain.configs;
// using domain.repository;
// using System;
// using System.Collections.Generic;
// using System.Text;
// using Dapper;
// using Microsoft.Extensions.Options;

// namespace application.services
// {
//     public class SignService : BaseServiceLfex, ISignService
//     {
//         public SignService(IOptionsMonitor<ConnectionStringList> connectionStringList) : base(connectionStringList) { }

//         public bool AddSign(String Sign, DateTime ServerTime, DateTime ClientTime, String Controller, String Action)
//         {
//             try
//             {
//                 var Row = base.dbConnection.Execute($"INSERT INTO `yoyo_sign_record` (`Sign`, `ServerTime`, `ClientTime`, `ControllerName`, `ActionName`)  VALUES (@Sign, @ServerTime, @ClientTime, @Controller,@Action)", new { Sign, ServerTime, ClientTime, Controller, Action });
//                 return Row == 1;
//             }
//             catch (Exception ex)
//             {
//                 Yoyo.Core.SystemLog.Debug("签名记录错误", ex);
//                 return false;
//             }
//         }
//     }
// }
