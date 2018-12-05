using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.TeamFoundation.VersionControl.Client;
using smartHookah.Controllers;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Support;

namespace smartHookah.Services.Place
{
    public class PlaceService : IPlaceService
    {
        private readonly SmartHookahContext db;

        public PlaceService(SmartHookahContext db)
        {
            this.db = db;
        }

        public async Task<Models.Place> GetPlace(int id)
        {
            var place = await db.Places.FirstOrDefaultAsync(a => a.Id == id);
            if (place == null) throw new ItemNotFoundException($"Place with id {id} not found.");
            return place;
        }

        public async Task<IEnumerable<TobaccoReview>> GetPlaceTobaccoReviews(int id, int pageSize = 10, int page = 0) =>
            await db.TobaccoReviews
                .Where(a => a.SmokeSession.PlaceId != null && a.SmokeSession.PlaceId == id)
                .OrderByDescending(a => a.PublishDate)
                .Skip(pageSize * page).Take(pageSize).ToListAsync();

        public PlaceStatisticsDto GetPlaceStatisticsDto(string id, DateTime from, DateTime to)
        {
            var place = db.Places
                .Include(a => a.Reservations)
                .Include("Reservations.Seats")
                .FirstOrDefault(a => a.FriendlyUrl == id);

            if (place?.Reservations == null)
            {
                throw new ItemNotFoundException($"No data found for place {id}.");
            }

            var reservations = place.Reservations
                .Where(a => a.Status == ReservationState.VisitConfirmed || a.Status == ReservationState.Confirmed || a.Status == ReservationState.Created)
                .Where(a => a.Created >= from && a.Created <= to)
                .OrderBy(a => a.Created)
                .ToList();
            
            var dayDistribution = reservations.GroupBy(a => a.Time.DayOfWeek)
                .ToPlotData(a => a.ToString(), a => (int)a.Key);
            var timeDistribution = reservations.GroupBy(a => a.Time.Hour.ToString(), a => a)
                .ToPlotData(a => a.ToString());
            var groupSize = reservations.GroupBy(a => a.Persons.ToString(), a => a)
                .ToPlotData(a => a.ToString());
            var visitDuration = reservations.GroupBy(a => a.Duration.ToString(), a => a)
                .ToPlotData(a => a.ToString());
            var weekVisits = reservations.GroupBy(a => a.Time.GetIso8601WeekOfYear(), a => a)
                .ToPlotData(a => a.ToString());
            var monthVisit = reservations.GroupBy(a => a.Time.Month, a => a).ToPlotData(a => a.ToString());
            var tableUsage = reservations.GroupBy(a => a.Seats.FirstOrDefault()?.Name)
                .ToPlotData(a => a.ToString());

            return new PlaceStatisticsDto()
            {
                From = from.ToString(CultureInfo.CurrentCulture),
                To = to.ToString(CultureInfo.CurrentCulture),
                Customers = reservations.Sum(a => a.Persons),
                DataSpan = (to - from).TotalDays.ToString(CultureInfo.CurrentCulture),
                DayDistribution = dayDistribution,
                GroupSize = groupSize,
                MonthVisits = monthVisit,
                ReservationCount = reservations.Count(),
                TableUsage = tableUsage,
                TimeDistribution = timeDistribution,
                VisitDuration = visitDuration,
                WeekVisits = weekVisits
            };
        }
    }
}