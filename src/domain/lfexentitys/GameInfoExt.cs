using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GameInfoExt
    {
        public uint Id { get; set; }
        public string GameId { get; set; }
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
        public int CpUserId { get; set; }
        public int GamePlatform { get; set; }
        public string IpWhiteList { get; set; }
        public string CallbackUrl { get; set; }
        public int IsOpen { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
