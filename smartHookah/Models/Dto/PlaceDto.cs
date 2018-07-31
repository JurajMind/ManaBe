using System;
using System.Linq;

namespace smartHookah.Models.Dto
{
    public class HookahOrderDto
    {
        public DateTime Created { get; set; }

        public int Id { get; set; }

        public int PlaceId { get; set; }

        public int? PersonId { get; set; }

        public int SmokeSessionMetaDataId { get; set; }

        public int? SmokeSessionId { get; set; }

        public string ExtraInfo { get; set; }

        public OrderState State { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public int? SeatId { get; set; }

        public int? ReservationId { get; set; }

        public HookahOrderType Type { get; set; }

        public static HookahOrderDto FromModel(HookahOrder model)
        {
            return new HookahOrderDto()
            {
                Created = model.Created,
                Id = model.Id,
                PlaceId = model.PlaceId,
                PersonId = model.PersonId,
                SmokeSessionMetaDataId = model.SmokeSessionMetaDataId,
                SmokeSessionId = model.SmokeSessionId,
                ExtraInfo = model.ExtraInfo,
                State = model.State,
                Price = model.Price,
                Currency = model.Currency,
                SeatId = model.SeatId,
                ReservationId = model.ReservationId,
                Type = model.Type,
            };
        }

        public HookahOrder ToModel()
        {
            return new HookahOrder()
            {
                Created = Created,
                Id = Id,
                PlaceId = PlaceId,
                PersonId = PersonId,
                SmokeSessionMetaDataId = SmokeSessionMetaDataId,
                SmokeSessionId = SmokeSessionId,
                ExtraInfo = ExtraInfo,
                State = State,
                Price = Price,
                Currency = Currency,
                SeatId = SeatId,
                ReservationId = ReservationId,
                Type = Type,
            };
        }
    }

    public class ReservationDto
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public int? PersonId { get; set; }

        public int PlaceId { get; set; }

        public DateTime? Started { get; set; }

        public DateTime? End { get; set; }

        public ReservationState Status { get; set; }

        public int Persons { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime Time { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        public string DisplayName { get; private set; }

        public static ReservationDto FromModel(Reservation model)
        {
            return new ReservationDto()
            {
                Id = model.Id,
                Created = model.Created,
                PersonId = model.PersonId,
                PlaceId = model.PlaceId,
                Started = model.Started,
                End = model.End,
                Status = model.Status,
                Persons = model.Persons,
                Duration = model.Duration,
                Time = model.Time,
                Text = model.Text,
                Name = model.Name,
                DisplayName = model.DisplayName,
            };
        }

        public Reservation ToModel()
        {
            return new Reservation()
            {
                Id = Id,
                Created = Created,
                PersonId = PersonId,
                PlaceId = PlaceId,
                Started = Started,
                End = End,
                Status = Status,
                Persons = Persons,
                Duration = Duration,
                Time = Time,
                Text = Text,
                Name = Name,
            };
        }
    }
}