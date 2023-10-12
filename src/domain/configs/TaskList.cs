using System.Collections.Generic;

namespace domain.configs
{
    public class TaskList
    {
        public List<Tasks> TaskLists { get; set; }
    }
    public class Tasks
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsExchange { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsRenew { get; set; }
        public bool StoreShow { get; set; }
        public decimal Pow { get; set; }
        public int MinningId { get; set; }
        public string MinningName { get; set; }
        public int CandyIn { get; set; }
        public decimal CandyOut { get; set; }
        public string RunTime { get; set; }
        public decimal CandyH { get; set; }
        /// <summary>
        /// �Ŷӹ���
        /// </summary>
        public decimal TeamH { get; set; }
        public int CandyP { get; set; }
        public decimal DayCandyOut { get; set; }
        public int MaxHave { get; set; }
        public string Colors { get; set; }
        public string Remark { get; set; } = "";

    }
}