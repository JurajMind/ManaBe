using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace smartHookah.Controllers.Mobile
{
    using System.Data.Entity.Spatial;
    using System.Threading.Tasks;

    using smartHookah.Models;

    public class PlaceController : ApiController
    {
        private SmartHookahContext _db;
        public PlaceController(SmartHookahContext db)
        {
            _db = db;
        }
        public async Task<List<PlaceDTO>> List()
        {
            return new List<PlaceDTO>();
        }

        [HttpGet]
        public async Task<List<PlaceDTO>> Search(double lat , double lng)
        {
            var myLocation = DbGeography.FromText($"POINT({lat} {lng})");
            var closestPlaces =  (from u in this._db.Places
                                                 orderby u.Address.Location.Distance(myLocation)
                                                 select u).Take(5);
            var ids = closestPlaces.Select(a => a.FriendlyUrl).ToList();
            return new List<PlaceDTO>();
        }

        public async Task<PlaceDTO> Details(string id)
        {
            return new PlaceDTO();
        }

    
    }

    public class PlaceDTO
    {
    }
}
