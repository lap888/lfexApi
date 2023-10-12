namespace domain.models.yoyoDto
{
    public class GameListDto : BaseModel
    {
        public string Platform { get; set; } = "";
        public int Type { get; set; } = -1;
    }
}