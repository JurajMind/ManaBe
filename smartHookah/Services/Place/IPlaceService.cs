using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Services.Place
{
    public interface IPlaceService
    {
        Task<Models.Place> GetPlace(int id);
        Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0);
        PlaceStatisticsDto GetPlaceStatisticsDto(string id, DateTime from, DateTime to);
    }
}