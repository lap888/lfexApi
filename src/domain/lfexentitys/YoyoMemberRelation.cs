using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class YoyoMemberRelation
    {
        public long MemberId { get; set; }
        public long ParentId { get; set; }
        public int RelationLevel { get; set; }
        public string Topology { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
