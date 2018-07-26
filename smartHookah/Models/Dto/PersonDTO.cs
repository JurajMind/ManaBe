using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class PersonActiveDataDTO : DTO
    {
        public ICollection<Hookah> ActiveStands { get; set; }
        public ICollection<SmokeSession> ActiveSmokeSessions { get; set; }
        public ICollection<Reservation> ActiveReservations { get; set; }
        public ICollection<HookahOrder> ActiveHookahOrders { get; set; }
        public GameProfile GameProfile { get; set; }

        public PersonActiveDataDTO()
        {
            ActiveStands = new List<Hookah>();
            ActiveSmokeSessions = new List<SmokeSession>();
            ActiveReservations = new List<Reservation>();
            ActiveHookahOrders = new List<HookahOrder>();
            GameProfile = new GameProfile();
        }
    }
}