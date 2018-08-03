using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class PersonActiveDataDTO
    {
        public ICollection<HookahSimpleDto> Stands { get; set; }
        public ICollection<SmokeSessionSimpleDto> ActiveSmokeSessions { get; set; }
        public ICollection<ReservationDto> ActiveReservations { get; set; }
        public ICollection<HookahOrderDto> ActiveHookahOrders { get; set; }
         public GameProfileSimpleDto GameProfile { get; set; }

        public PersonActiveDataDTO()
        {
            Stands = new List<HookahSimpleDto>();
            ActiveSmokeSessions = new List<SmokeSessionSimpleDto>();
            ActiveReservations = new List<ReservationDto>();
            ActiveHookahOrders = new List<HookahOrderDto>();
            //GameProfile = new GameProfile();
        }
    }
}