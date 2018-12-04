using System;
using System.Collections.Generic;
using smartHookah.Models.Dto;

namespace smartHookah.Services.Statistics
{
    public interface IPersonStatisticsService
    {
        List<Models.SmokeSession> GetPersonSessions(DateTime? from, DateTime? to);

        SmokeSessionTimeStatisticsDto GetSessionTimeStatistics(IEnumerable<Models.SmokeSession> sessions);

        List<PipeAccessoryUsageDto> GetAccessoriesUsage(IEnumerable<Models.SmokeSession> session);
    }
}