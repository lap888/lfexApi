using domain.lfexentitys;

namespace domain.models.yoyoDto
{
    public class MinningDto : Minnings
    {
        public string MinningName { get; set; }
        public int CandyIn { get; set; }
        public decimal CandyOut { get; set; }
        public string RunTime { get; set; }
        public decimal CandyH { get; set; }
        public int CandyP { get; set; }
        public decimal DayCandyOut { get; set; }
        public int MaxHave { get; set; }
        public string Colors { get; set; }
    }
}