using System.Collections.Generic;

namespace smartHookah.Models.Dto.Places
{
    public class PlaceDashboardDto
    {
        public PlaceDashboardDto()
        {
            this.PlaceDevices = new List<DevicePlaceDashboardDto>();
        }
        public ICollection<DevicePlaceDashboardDto> PlaceDevices { get; set; }
    }

    public class DevicePlaceDashboardDto
    {
        public DeviceSimpleDto Device { get; set; }

        public DynamicSmokeStatisticRawDto Statistic { get; set; }

        public SmokeSessionMetaDataDto MetaData { get; set; }

        public decimal TobaccoEstimate { get; set; }

    }
}