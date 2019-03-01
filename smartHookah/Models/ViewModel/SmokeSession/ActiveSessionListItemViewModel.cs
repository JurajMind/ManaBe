using smartHookah.Models.Db;
using smartHookah.Models.Redis;

namespace smartHookah.Models
{
    public class ActiveSessionListItemViewModel
    {
        public int SessionId { get; set; }
        public DynamicSmokeStatistic DynamicSmokeSession { get; set; }
        public SmokeSession SmokeSession { get; set; }
    }
}