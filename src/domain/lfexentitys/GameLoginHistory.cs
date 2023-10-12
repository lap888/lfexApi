using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GameLoginHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameAppid { get; set; }
        public string Source { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }
        public int RoleLevel { get; set; }
        public int ServerId { get; set; }
        public string ServerName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
