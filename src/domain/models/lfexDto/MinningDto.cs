using System;

namespace domain.models.lfexDto
{
    public class MinningDto 
    {
        /// <summary>
        /// 矿机名称
        /// </summary>
        /// <value></value>
        public string MinningName { get; set; }
        /// <summary>
        /// 矿机算力
        /// </summary>
        /// <value></value>
        public decimal Pow { get; set; }
        /// <summary>
        /// 矿机颜色
        /// </summary>
        /// <value></value>
        public string Colors { get; set; }

        public int Id { get; set; }
        public long UserId { get; set; }
        /// <summary>
        /// 矿机ID
        /// </summary>
        /// <value></value>
        public int MinningId { get; set; }
        /// <summary>
        /// 矿机运行状态 0未挖矿 1正在挖矿 2已收取
        /// </summary>
        /// <value></value>
        public int MinningStatus { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public sbyte? Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// 当日挖矿时间
        /// </summary>
        /// <value></value>
        public string WorkingTime { get; set; }
        /// <summary>
        /// 挖矿结束时间
        /// </summary>
        /// <value></value>
        public string WorkingEndTime { get; set; }
        
        /// <summary>
        /// 矿机类型
        /// </summary>
        /// <value></value>
        public int? Source { get; set; }
    }
}