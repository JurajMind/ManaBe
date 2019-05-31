using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using smartHookah.ErrorHandler;
using smartHookah.Models.Redis;
using smartHookah.Services.Redis;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/Competition")]
    public class CompetitionController : ApiController
    {
        private readonly IRedisService redisService;

        public CompetitionController(IRedisService redisService)
        {
            this.redisService = redisService;
        }

        [HttpPost, ApiAuthorize, Route("Add")]
        public bool AddEntry(string name, double time)
        {
            var entry = new CompetitionEntry()
            {
                Time = time,
                Name = name
            };

            this.redisService.AddCompetitionEntry(entry);

            return true;
        }

        [HttpGet, ApiAuthorize, Route("Results")]
        public List<CompetitionEntry> GetResults()
        {
            return this.redisService.GetCompetitionEntries().OrderByDescending(a => a.Time).ToList();
        }

        [HttpDelete, ApiAuthorize, Route("Clean")]
        public bool Clean()
        {
            this.redisService.CleanCompetition();
            return true;
        }
    }
}
