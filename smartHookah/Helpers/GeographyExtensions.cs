using System;
using System.Data.Entity.Spatial;
using System.Globalization;

namespace smartHookah.Helpers
{
    public static class GeographyExtensions
    {
        public static DbGeography CreatePoint(double latitude, double longitude)
        {
            return DbGeography.FromText(String.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", longitude, latitude));
        }

        public static DbGeography CreatePoint(string latitude, string longitude)
        {
            return DbGeography.FromText(String.Format(CultureInfo.InvariantCulture, "POINT({0} {1})", longitude, latitude));
        }
    }
}