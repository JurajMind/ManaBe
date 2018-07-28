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
        // public ICollection<Reservation> ActiveReservations { get; set; }
        // public ICollection<HookahOrder> ActiveHookahOrders { get; set; }
        // public GameProfile GameProfile { get; set; }

        public PersonActiveDataDTO()
        {
            Stands = new List<HookahSimpleDto>();
            ActiveSmokeSessions = new List<SmokeSessionSimpleDto>();
            //ActiveReservations = new List<Reservation>();
            //ActiveHookahOrders = new List<HookahOrder>();
            //GameProfile = new GameProfile();
        }
    }
}