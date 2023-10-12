using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class SysClientVersions
    {
        public uint Id { get; set; }
        public string CurrentVersion { get; set; }
        public sbyte IsSilent { get; set; }
        public sbyte IsHotReload { get; set; }
        public string DeviceSystem { get; set; }
        public string UpdateContent { get; set; }
        public sbyte Production { get; set; }
        public sbyte IsNecessary { get; set; }
        public string DownloadUrl { get; set; }
    }
}
