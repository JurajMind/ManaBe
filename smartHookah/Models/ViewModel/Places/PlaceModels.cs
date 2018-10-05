using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using smartHookah.Controllers;
using smartHookah.Models;
using smartHookah.Models.Redis;

namespace smartHookah.Models
{
    using System.Globalization;

    public class SeatDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public SeatDto(Seat seat)
        {
            this.Code = seat.Code;
            this.Name = seat.Name;
        }
    }

    public class EditPricePostModel
    {
        public int PlaceId { get; set; }

        public List<EditPricePostItemPriceGroup> priceGroup { get; set; }

        public string Currency { get; set; }
        public List<EditPricePostItems> Items { get; set; }

        public decimal BasePrice { get; set; }
    }

    public class EditPricePostItems
    {
        public int Id { get; set; }

        public string Currency { get; set; }
        
        public List<EditPricePostItemPriceGroup> Prices { get; set; }
    }

    public class EditPricePostItemPriceGroup
    {
        public int Id { get; set; }
        public string Price { get; set; }

        public decimal PriceValue
        {
            get
            {
                if (this.Price == null) return -1;
                decimal price;
                this.Price = this.Price.Replace(',', '.');
                decimal.TryParse(this.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out price);


                return price;
            }
        }
    }

    public class EditPriceViewModel
    {
        public List<OwnPipeAccesories> OwnedGear { get; set; }
        public List<OrderExtra> Extras { get; set; }
        public int PlaceId { get; set; }
        public List<PriceGroup> PriceGroups { get; set; }
        public decimal BasePrice { get; set; }
        public string Currency { get; set; }
    }

    public class OrderExtraDto
    {
        public int Id { get; set; }
        public OrderExtraDto(OrderExtra orderExtra)
        {
            this.Id = orderExtra.Id;
            this.Name = orderExtra.Name;
            if (orderExtra.Price != null)
            {
                this.Price = orderExtra.Price;
                this.Currency = orderExtra.Currency;
            }

        }

        public string Currency { get; set; }

        public decimal Price { get; set; }

        public string Name { get; set; }
    }

    public class ProcessOrderViewModel
    {
        public HookahOrder order;
        public SmokeMetadataModalViewModel SmokeMetadataModalViewModel { get; set; }
        public List<Hookah> Hookahs { get; set; }

        public List<Seat> Seats { get; set; }

        public int SelectedSeat { get; set; }

    }

    public class OrderDetailsViewModel
    {
        public HookahOrder order { get; set; }

    }

    public class PlaceIndexViewModel
    {
        public int? ownPlaceId { get; set; }
        public List<Place> Places { get; set; }
    }

    public class HookahDashboardViewModel
    {
        public string Key { get; set; }

        public string Name { get; set; }

        public DynamicSmokeStatisticDto DynamicSmokeStatisticDto { get; set; } =
            new DynamicSmokeStatisticDto(new DynamicSmokeStatistic());

        public double EstPufCount { get; set; }

        public bool Online { get; set; }

        public bool Service { get; set; }

        public string Table { get; set; }
        public string HookahPicture { get; set; }
    }

    public class LoungeDashBoardViewModel
    {
        public List<HookahDashboardViewModel> Hookah { get; set; }
        public List<string> OnlineHookah { get; set; }
        public Dictionary<string, DynamicSmokeStatistic> DynamicStatistic { get; set; }
        public Place Place { get; set; }
    }

    public class EditPriceItemModel
    {
        public OwnPipeAccesories Item { get; set; }
        public int Index { get; set; }

        public List<PriceGroup> PriceGroups { get; set; }
    }
}