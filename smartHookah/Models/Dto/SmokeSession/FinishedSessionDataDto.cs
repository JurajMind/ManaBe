namespace smartHookah.Models.Dto
{
    public class FinishedSessionDataDto
    {
        public SmokeSessionSimpleDto Data { get; set; }
        public SmokeSessionMetaDataDto MetaData { get; set; }

        public SmokeSessionStatisticsDto Statistics { get; set; }
    }
}