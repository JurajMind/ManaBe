using System;
using System.Collections.Generic;
using System.Linq;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Support;

namespace smartHookah.Models.Dto.Places.Reservations
{
    public class ReservationDto
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public int? PersonId { get; set; }
        
        public int PlaceId { get; set; }

        public string PlaceName { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? End { get; set; }
        
        public int Persons { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime Time { get; set; }
        
        public string Text { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; private set; }

        public int Status { get; set; }

        public List<int> Seats { get; set; }
        public int? LateDuration { get; set; }

        public static ReservationDto FromModel(Reservation model) => model == null
            ? null
            : new ReservationDto()
            {
                Id = model.Id,
                Created = model.Created,
                PersonId = model.PersonId,
                PlaceId = model.PlaceId,
                PlaceName = model.Place.Name,
                Started = model.Started,
                End = model.End,
                Persons = model.Persons,
                Duration = model.Duration,
                Time = model.Time,
                Text = model.Text,
                Name = model.Name,
                DisplayName = model.DisplayName,
                Seats = model.Seats.EmptyIfNull().Select(a => a.Id).ToList(),
                Status = (int)model.Status,
                LateDuration = model.LateDuration
            };

        public static Reservation ToModel(ReservationDto dto)
        {
            return new Reservation
            {
                PersonId = dto.PersonId,
                Created = dto.Created,
                Persons = dto.Persons,
                Started = dto.Started,
                Time = dto.Time,
                Duration = dto.Duration,
                Id = dto.Id,
                Name = dto.Name,
                Text = dto.Text,
                PlaceId = dto.PlaceId,
                Status = (ReservationState)dto.Status,
                Seats = new List<Seat>(),
                LateDuration = dto.LateDuration,
            };
        }

        public static Reservation ToModel(ReservationDto dto, IEnumerable<Seat> seats)
        {
            var reservation = ToModel(dto);
            if (dto.Seats.Count > 0)
            {
                reservation.Seats = seats.Where(s => dto.Seats.Contains(s.Id)).ToList();
            }

            return reservation;
        }

        public static IEnumerable<ReservationDto> FromModelList(IEnumerable<Reservation> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }
    }
}