using smartHookah.Helpers;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;
using System;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using PlaceDay = smartHookah.Models.Dto.PlaceDay;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/PlaceCalendar")]
    public class PlaceCalendarController : ApiController
    {
        private readonly SmartHookahContext _db;

        public PlaceCalendarController(SmartHookahContext db)
        {
            this._db = db;
        }

        #region Creators and editors

        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SavePlaceDay")]
        public async Task<DTO> SavePlaceDay([Bind(Include = "Id,PlaceId,Day,OpenHour,CloseHour")] PlaceDay placeDay)
        {
            if (placeDay == null) return new DTO() { Success = false, Message = "PlaceDay is null." };

            var day = _db.PlaceDays.Find(placeDay.Id);
            var place = _db.Places.Find(placeDay.PlaceId);

            if (place == null) return new DTO() { Success = false, Message = "Place not found, please provide correct place id." };

            try
            {
                day = new Models.Db.PlaceDay() { Id = placeDay.Id > 0 ? placeDay.Id : 0, Day = placeDay.Day, PlaceId = placeDay.PlaceId, OpenHour = placeDay.OpenHour, CloseHour = placeDay.CloseHour };

                _db.PlaceDays.AddOrUpdate(day);
                _db.SaveChanges();

                return new DTO() { Success = true, Message = "PlaceDay successfully saved." };
            }
            catch (Exception e)
            {
                return new DTO() { Success = false, Message = $"{e.Message}" };
            }
        }

        [System.Web.Http.AcceptVerbs("POST")]
        [System.Web.Http.Route("SavePlaceEvent")]
        public async Task<DTO> SavePlaceEvent([Bind(Include = "Id,PlaceDayId,Title,Description,Start,End,PrivacyType,FacebookUrl")] PlaceEventDTO placeEvent)
        {
            if (placeEvent == null) return new DTO() { Success = false, Message = "PlaceEvent is null." };

            var day = _db.PlaceDays.Find(placeEvent.PlaceDayId);

            if (day == null) return new DTO() { Success = false, Message = "PlaceDay not found, please provide correct PlaceDay id." };
            if (placeEvent.Start > placeEvent.End) return new DTO() { Success = false, Message = "Event cannot end after it starts." };
            if (placeEvent.Start.Date != day.Day.Date) return new DTO() { Success = false, Message = "Event start date must be the same as corresponding PlaceDay date." };

            if (!FacebookHelper.ValidateEventPrivacyType(placeEvent.PrivacyType)) return new DTO() { Success = false, Message = "Event privacy type not valid." };

            try
            {
                var ev = new PlaceEvent()
                {
                    Id = placeEvent.Id > 0 ? placeEvent.Id : 0,
                    Title = placeEvent.Title,
                    Description = placeEvent.Description,
                    Start = placeEvent.Start,
                    End = placeEvent.End,
                    FacebookUrl = placeEvent.FacebookUrl,
                    PlaceDayId = day?.Id ?? 0,
                    PrivacyType = placeEvent.PrivacyType
                };

                _db.PlaceEvents.AddOrUpdate(ev);
                _db.SaveChanges();

                return new DTO() { Success = true, Message = "PlaceEvent successfully saved." };
            }
            catch (Exception e)
            {
                return new DTO() { Success = false, Message = $"{e.Message}" };
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("CreateEventFromFacebook")]
        public async Task<DTO> CreatePlaceEventFromFacebook(string url)
        {
            return new DTO() { Success = false, Message = "Function not implemented yet." };
        }

        #endregion

        #region Getters and finders

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetCalendar")]
        public async Task<PlaceCalendarDto> GetPlaceCalendar(int placeId, string privacyType = "public")
        {
            if (placeId < 0)
                throw new ManaException(ErrorCodes.PlaceNotFound, "Wrong place id");
            var place = _db.Places.Find(placeId);
            if (place == null)
                throw new ManaException(ErrorCodes.PlaceNotFound, "Wrong place id");

            if (!FacebookHelper.ValidateEventPrivacyType(privacyType))
                throw new ManaException(ErrorCodes.InvalidPrivacyType, "Invalid privaci type");

            var result = new PlaceCalendarDto();
            foreach (var day in place.PlaceDays)
            {
                var d = new PlaceDay() { Id = day.Id, PlaceId = day.PlaceId, Day = day.Day, OpenHour = day.OpenHour, CloseHour = day.CloseHour };
                foreach (var e in day.PlaceEvents)
                {
                    if (e.PrivacyType.ToLower() == privacyType.ToLower() && FacebookHelper.ValidateEventPrivacyType(privacyType))
                    {
                        var ev = new PlaceEventDTO()
                        {
                            Id = e.Id,
                            Title = e.Title,
                            Description = e.Description,
                            Start = e.Start,
                            End = e.End,
                            FacebookUrl = e.FacebookUrl,
                            PlaceDayId = e.PlaceDayId,
                            PrivacyType = e.PrivacyType
                        };
                        d.PlaceEvents.Add(ev);
                    }
                }
                result.PlaceDays.Add(d);
            }

            return result;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetPlaceDay")]
        public async Task<PlaceDay> GetPlaceDay(int placeId, int? dayId, DateTime? dayDate, string privacyType = "public")
        {
            if (placeId < 1)
                throw new ManaException(ErrorCodes.PlaceNotFound, "Wrong place id");
            var place = _db.Places.Find(placeId);
            if (place == null)
                throw new ManaException(ErrorCodes.PlaceNotFound, "Wrong place id");
            if (!FacebookHelper.ValidateEventPrivacyType(privacyType))
                throw new ManaException(ErrorCodes.InvalidPrivacyType, "Invalid privaci type");

            var day = new Models.Db.PlaceDay();

            if (dayId.HasValue)
            {
                day = (from d in place.PlaceDays where d.Id == dayId select d).FirstOrDefault();
            }
            else if (dayDate.HasValue)
            {
                day = (from d in place.PlaceDays where d.Day.Date == dayDate select d).FirstOrDefault();
            }
            else
            {
                throw new ManaException(ErrorCodes.WrongDay, "Wrong date");
            }

            if (day != null)
            {
                var result = new PlaceDay() { Id = day.Id, PlaceId = day.PlaceId, Day = day.Day, OpenHour = day.OpenHour, CloseHour = day.CloseHour };

                foreach (var e in day.PlaceEvents)
                {
                    if (e.PrivacyType.ToLower() == privacyType.ToLower() && FacebookHelper.ValidateEventPrivacyType(privacyType))
                    {
                        var placeEvent = new PlaceEventDTO()
                        {
                            Id = e.Id,
                            Title = e.Title,
                            Description = e.Description,
                            Start = e.Start,
                            End = e.End,
                            FacebookUrl = e.FacebookUrl,
                            PlaceDayId = e.PlaceDayId,
                            PrivacyType = e.PrivacyType
                        };
                        result.PlaceEvents.Add(placeEvent);
                    }
                }
                return result;
            }
            throw new ManaException(ErrorCodes.WrongDay, "Wrong date");
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetPlaceEvent")]
        public async Task<PlaceEventDTO> GetPlaceEvent(int eventId)
        {
            if (eventId < 1) return new PlaceEventDTO() { Success = false, Message = "Please provide correct PlaceEvent id." };

            var e = _db.PlaceEvents.Find(eventId);

            if (e == null) return new PlaceEventDTO() { Success = false, Message = "Event not found." };

            var result = new PlaceEventDTO()
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Start = e.Start,
                End = e.End,
                FacebookUrl = e.FacebookUrl,
                PlaceDayId = e.PlaceDayId,
                PrivacyType = e.PrivacyType
            };
            return result;
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("FindEventsBy")]
        public async Task<PlaceEventCollectionDTO> FindEventsBy(int? placeId, DateTime? startsAfter, DateTime? endsBefore, string privacyType = "")
        {
            var query = from ev in _db.PlaceEvents select ev;

            query = (placeId.HasValue && placeId > 0 && _db.Places.Find(placeId) != null)
                ? query.Where(e => e.PlaceDay.PlaceId == placeId)
                : query;
            query = startsAfter.HasValue
                ? query.Where(e => DbFunctions.TruncateTime(e.Start) >= DbFunctions.TruncateTime(startsAfter))
                : query.Where(e => DbFunctions.TruncateTime(e.Start) >= DbFunctions.TruncateTime(DateTime.UtcNow));
            query = endsBefore.HasValue
                ? query.Where(e => DbFunctions.TruncateTime(e.Start) <= DbFunctions.TruncateTime(endsBefore))
                : query.Where(e => DbFunctions.TruncateTime(e.Start) <= DbFunctions.AddMonths(DateTime.UtcNow, 6));
            query = FacebookHelper.ValidateEventPrivacyType(privacyType)
                ? query.Where(e => e.PrivacyType.ToLower() == privacyType.ToLower())
                : query.Where(e => e.PrivacyType.ToLower() == "public");

            var events = query.ToList();

            if (events.Count > 0)
            {
                var result = new PlaceEventCollectionDTO() { Success = true, Message = $"{events.Count} events found." };

                foreach (var e in events)
                {
                    var eventDto = new PlaceEventDTO()
                    {
                        Id = e.Id,
                        Title = e.Title,
                        Description = e.Description,
                        Start = e.Start,
                        End = e.End,
                        FacebookUrl = e.FacebookUrl,
                        PlaceDayId = e.PlaceDayId,
                        PrivacyType = e.PrivacyType
                    };
                    result.EventCollection.Add(eventDto);
                }

                return result;
            }

            return new PlaceEventCollectionDTO() { Success = false, Message = "No events with given parameters found." };
        }

        #endregion

        #region Deleters

        [System.Web.Http.AcceptVerbs("DELETE")]
        [System.Web.Http.Route("DeletePlaceDay")]
        public async Task<DTO> DeletePlaceDay(int dayId)
        {
            var day = _db.PlaceDays.FirstOrDefault(d => d.Id == dayId);

            if (day != null)
            {
                try
                {
                    _db.PlaceDays.Remove(day);
                    _db.SaveChanges();
                    return new DTO() { Success = true, Message = $"PlaceDay with id {dayId} has been deleted." };
                }
                catch (Exception e)
                {
                    return new DTO() { Success = false, Message = e.Message };
                }
            }

            return new DTO() { Success = false, Message = $"Cannot find PlaceDay with id {dayId}." };
        }

        [System.Web.Http.AcceptVerbs("DELETE")]
        [System.Web.Http.Route("DeletePlaceEvent")]
        public async Task<DTO> DeletePlaceEvent(int eventId)
        {
            var ev = _db.PlaceEvents.FirstOrDefault(d => d.Id == eventId);

            if (ev != null)
            {
                try
                {
                    _db.PlaceEvents.Remove(ev);
                    _db.SaveChanges();
                    return new DTO() { Success = true, Message = $"PlaceEvent with id {eventId} has been deleted." };
                }
                catch (Exception e)
                {
                    return new DTO() { Success = false, Message = e.Message };
                }
            }

            return new DTO() { Success = false, Message = $"Cannot find PlaceEvent with id {eventId}." };
        }

        #endregion
    }
}
