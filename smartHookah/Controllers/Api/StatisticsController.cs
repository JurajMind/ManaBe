using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using smartHookah.Models.Dto;
using smartHookah.Services.Statistics;

namespace smartHookah.Controllers.Api
{
    [RoutePrefix("api/Statistics")]
    public class StatisticsController : ApiController
    {
        private readonly IPersonStatisticsService _personStatisticsService;

        public StatisticsController(IPersonStatisticsService personStatisticsService)
        {
            this._personStatisticsService = personStatisticsService;
        }

        #region Getters

        [HttpGet, Route("GetStatistics")]
        public PersonStatisticsOverallDto GetStatistics(DateTime? from, DateTime? to)
        {
            if (to == null)
            {
                to = DateTime.UtcNow;
            }

            if (from == null)
            {
                from = DateTime.UtcNow.AddMonths(-1);
            }

            try
            {
                var sessions = _personStatisticsService.GetPersonSessions(from, to);
                var result = new PersonStatisticsOverallDto()
                {
                    SmokeSessions = SmokeSessionSimpleDto.FromModelList(sessions),
                    SmokeSessionTimeStatistics = _personStatisticsService.GetSessionTimeStatistics(sessions),
                    PipeAccessoriesUsage = _personStatisticsService.GetAccessoriesUsage(sessions)
                };

                return result;
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, err));
            }
        }

        #endregion

    }
}
