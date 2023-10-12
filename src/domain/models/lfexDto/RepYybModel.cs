namespace domain.models.lfexDto
{
    public class RepYybModel<T>
    {
        public int ErrCode { get; set; }
        public string ErrMsg { get; set; }
        public int Timestamp { get; set; }
        public string Sign { get; set; }
        public T Data { get; set; }
    }
}