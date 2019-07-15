using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using smartHookah.Models.Db;
using smartHookah.Models.Db.Place;
using smartHookah.Models.Dto.Places.Reservations;
using smartHookah.Support;

namespace smartHookah.Models.Dto
{
    public class PlaceDto : PlaceSimpleDto
    {
        public string Url { get; set; }
        
        public string Descriptions { get; set; }

        public decimal BaseHookahPrice { get; set; }

        public string Currency { get; set; }

        public IEnumerable<SeatDto> Seats { get; set; }
        
        public IEnumerable<TobaccoReviewDto> TobaccoReviews { get; set; }

        public IEnumerable<MediaDto> Medias { get; set; }
        
        public bool HaveReservation { get; set; }

        public int MinimumReservationTime { get; set; }
        
        public int? FranchiseId { get; set; }

        public FranchiseDto Franchise { get; set; }

        public List<string> Flags { get; set; }

        public List<SocialMedia> SocialMedias { get; set; }

        public new static PlaceDto FromModel(Place model) => model == null
            ? null
            : new PlaceDto()
            {
                Id = model.Id,
                Name = model.Name,
                LogoPath = model.LogoPath,
                ShortDescriptions = model.ShortDescriptions,
                Descriptions = model.Descriptions,
                FriendlyUrl = model.FriendlyUrl,
                Address = AddressDto.FromModel(model.Address),
                BusinessHours = BusinessHoursDto.FromModelList(model.BusinessHours).ToList(),
                PhoneNumber = model.PhoneNumber,
                Facebook = model.Facebook,
                BaseHookahPrice = model.BaseHookahPrice,
                Currency = model.Currency,
                Seats = SeatDto.FromModelList(model.Seats),
                Medias = MediaDto.FromModelList(model.Medias),
                HaveReservation = model.HaveReservation,
                HaveMana = model.HaveMana,
                HaveOrders = model.HaveOrders,
                HaveMenu = model.HaveMenu,
                Rating = model.Rating,
                MinimumReservationTime = model.MinimumReservationTime,
                FranchiseId = model.FranchiseId,
                Franchise = FranchiseDto.FromModel(model.Franchise),
                Flags = model.PlaceFlags.Select(s => s.Code).ToList(),
                SocialMedias = model.SocialMedias.EmptyIfNull().ToList()
            };

        public Place ToModel(int? creator)
        {
            return new Place()
            {
                Id = this.Id,
                Name = this.Name,
                CreatorId = creator,
                CreatedAt =  DateTime.UtcNow,
                LogoPath = this.LogoPath,
                ShortDescriptions = this.ShortDescriptions,
                Descriptions = this.Descriptions,
                FriendlyUrl = Guid.NewGuid().ToString().Substring(0,20),
                Address = this.Address.ToModel(),
                PhoneNumber = this.PhoneNumber,
                Facebook = this.Facebook,
                HaveReservation = false,
                MinimumReservationTime = 5,
                BusinessHours = this.BusinessHours.Select(a => a.ToModel()).ToList(),
                
            };
        }
    }

    public class SeatDto
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Code { get; set; }

        public int Capacity { get; set; }

        public static SeatDto FromModel(Seat model) => model == null
            ? null
            : new SeatDto()
            {
                Id = model.Id,
                Name = model.Name,
                Code = model.Code,
                Capacity = model.Capacity,
            };

        public static IEnumerable<SeatDto> FromModelList(ICollection<Seat> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public Seat ToModel()
        {
            return new Seat()
            {
                Id = this.Id,
                Name = this.Name,
                Code = this.Code,
                Capacity = this.Capacity,
            };
        }

        public static IEnumerable<Seat> ToModelList(IEnumerable<SeatDto> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return item.ToModel();
            }
        }
    }

    public class HookahOrderDto
    {
        public DateTime Created { get; set; }

        public int Id { get; set; }

        public int PlaceId { get; set; }

        public int? PersonId { get; set; }

        public int SmokeSessionMetaDataId { get; set; }

        public SmokeSessionMetaDataDto SmokeSessionMetaData { get; set; }

        public int? SmokeSessionId { get; set; }

        public SmokeSessionSimpleDto SmokeSession { get; set; }

        public string ExtraInfo { get; set; }

        public OrderState State { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; }

        public int? SeatId { get; set; }

        public SeatDto Seat { get; set; }

        public int? ReservationId { get; set; }

        public ReservationDto Reservation { get; set; }

        public HookahOrderType Type { get; set; }

        public static HookahOrderDto FromModel(HookahOrder model) => model == null
            ? null
            : new HookahOrderDto()
            {
                Created = model.Created,
                Id = model.Id,
                PlaceId = model.PlaceId,
                PersonId = model.PersonId,
                SmokeSessionMetaDataId = model.SmokeSessionMetaDataId,
                SmokeSessionMetaData = SmokeSessionMetaDataDto.FromModel(model.SmokeSessionMetaData),
                SmokeSessionId = model.SmokeSessionId,
                SmokeSession = SmokeSessionSimpleDto.FromModel(model.SmokeSession),
                ExtraInfo = model.ExtraInfo,
                State = model.State,
                Price = model.Price,
                Currency = model.Currency,
                SeatId = model.SeatId,
                Seat = SeatDto.FromModel(model.Seat),
                ReservationId = model.ReservationId,
                Reservation = ReservationDto.FromModel(model.Reservation),
                Type = model.Type,
            };
        

        public static IEnumerable<HookahOrderDto> FromModelList(ICollection<HookahOrder> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public HookahOrder ToModel()
        {
            return new HookahOrder()
            {
                Created = this.Created,
                Id = this.Id,
                PlaceId = this.PlaceId,
                PersonId = this.PersonId,
                SmokeSessionMetaDataId = this.SmokeSessionMetaDataId,
                SmokeSessionMetaData = this.SmokeSessionMetaData.ToModel(),
                SmokeSessionId = this.SmokeSessionId,
                ExtraInfo = this.ExtraInfo,
                State = this.State,
                Price = this.Price,
                Currency = this.Currency,
                SeatId = this.SeatId,
                Seat = this.Seat.ToModel(),
                ReservationId = this.ReservationId,
                Reservation = ReservationDto.ToModel(this.Reservation),
                Type = this.Type,
            };
        }
    }

    public class BusinessHoursDto
    {
        public int Id { get; set; }

        public int PlaceId { get; set; }

        public int Day { get; set; }

        public TimeSpan OpenTine { get; set; }

        public TimeSpan CloseTime { get; set; }

        public static BusinessHoursDto FromModel(BusinessHours model) => model == null
            ? null
            : new BusinessHoursDto()
            {
                Id = model.Id,
                PlaceId = model.PlaceId,
                Day = model.Day,
                OpenTine = model.OpenTine,
                CloseTime = model.CloseTime,
            };

        public static IEnumerable<BusinessHoursDto> FromModelList(IEnumerable<BusinessHours> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public BusinessHours ToModel()
        {
            return new BusinessHours()
            {
                Id = this.Id,
                PlaceId = this.PlaceId,
                Day = this.Day,
                OpenTine = this.OpenTine,
                CloseTime = this.CloseTime,
            };
        }
    }

    public class AddressDto
    {
        public int Id { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string Number { get; set; }

        public string ZIP { get; set; }

        public string Lat { get; set; }

        public string Lng { get; set; }

        public static AddressDto FromModel(Address model) => model == null
            ? null
            : new AddressDto()
            {
                Id = model.Id,
                Street = model.Street,
                City = model.City,
                Number = model.Number,
                ZIP = model.ZIP,
                Lat = model.Lat,
                Lng = model.Lng
            };

        public Address ToModel()
        {
            return new Address()
            {
                Id = this.Id,
                Street = this.Street,
                City = this.City,
                Number = this.Number,
                ZIP = this.ZIP,
                Lat = this.Lat,
                Lng = this.Lng
            };
        }
    }

    public class MediaDto
    {
        public int Id { get; set; }

        public DateTime Created { get; set; }

        public string Path { get; set; }

        public MediaType Type { get; set; }

        public bool IsDefault { get; set; }

        public string Extension { get; private set; }

        public string FileName { get; private set; }

        public string Sizes { get; set; }



        public static MediaDto FromModel(Media model) => model == null
            ? null
            : new MediaDto()
            {
                Id = model.Id,
                Created = model.Created,
                Path = model.Path,
                Type = model.Type,
                IsDefault = model.IsDefault,
                Extension = model.Extension,
                FileName = model.FileName,
                Sizes = model.Sizes
            };

        public Media ToModel()
        {
            return new Media()
            {
                Id = this.Id,
                Created = this.Created,
                Path = this.Path,
                Type = this.Type,
                IsDefault = this.IsDefault,
            };
        }

        public static IEnumerable<MediaDto> FromModelList(ICollection<Media> model)
        {
            if (model == null) yield break;
            foreach (var item in model)
                yield return FromModel(item);
        }
    }

    public class FranchiseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Uril { get; set; }

        public static FranchiseDto FromModel(Franchise model) => model == null
            ? null
            : new FranchiseDto()
            {
                Id = model.Id,
                Name = model.Name,
                Uril = model.Uril,
            };
        

        public Franchise ToModel()
        {
            return new Franchise()
            {
                Id = this.Id,
                Name = this.Name,
                Uril = this.Uril,
            };
        }
    }
}