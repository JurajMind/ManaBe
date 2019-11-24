using smartHookah.Models.Dto;
using System;
using System.Collections.Generic;

namespace smartHookah.Services.Statistics
{
    public interface IPersonStatisticsService
    {
        List<Models.Db.SmokeSession> GetPersonSessions(DateTime? from, DateTime? to);

        SmokeSessionTimeStatisticsDto GetSessionTimeStatistics(IEnumerable<Models.Db.SmokeSession> sessions);

        List<PipeAccessoryUsageDto> GetAccessoriesUsage(IEnumerable<Models.Db.SmokeSession> session);
    }
}