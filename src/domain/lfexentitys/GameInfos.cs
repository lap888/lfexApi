using System;
using System.Collections.Generic;

namespace domain.lfexentitys
{
    public partial class GameInfos
    {
        public long Id { get; set; }
        public string GType { get; set; }
        public int? GPlatform { get; set; }
        public string GTitle { get; set; }
        public sbyte? IsFirstPublish { get; set; }
        public string Synopsis { get; set; }
        public string GtProportionl { get; set; }
        public string GtVip { get; set; }
        public string GPinyin { get; set; }
        public string GameCategoryId { get; set; }
        public decimal? GSize { get; set; }
        public string GVersion { get; set; }
        public decimal? Discount { get; set; }
        public string Description { get; set; }
        public string GH5url { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? GameSupplierId { get; set; }
        public sbyte? UseGem { get; set; }
        public float? UseGemRate { get; set; }
        public sbyte? IsShow { get; set; }
        public float? CompanyShareRatio { get; set; }
        public int SdwId { get; set; }
        public decimal? GSort { get; set; }
    }
}
