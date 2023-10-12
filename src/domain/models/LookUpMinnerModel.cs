namespace domain.models
{
    public class LooKUpMinnerModel : BaseModel
    {
        /// <summary>
        /// 0 锁仓中 1 已赎回
        /// </summary>
        /// <value></value>
        public int Status { get; set; }
    }
}