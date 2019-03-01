using System;
using System.Collections.Generic;
using smartHookah.Models.Dto;

namespace smartHookah.Services.Statistics
{
    public interface IPersonStatisticsService
    {
        List<Models.Db.SmokeSession> GetPersonSessions(DateTime? from, DateTime? to);

        SmokeSessionTimeStatisticsDto GetSessionTimeStatistics(IEnumerable<Models.Db.SmokeSession> sessions);

        List<PipeAccessoryUsageDto> GetAccessoriesUsage(IEnumerable<Models.Db.SmokeSession> session);
    }
}