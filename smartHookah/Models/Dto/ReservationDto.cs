using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    using smartHookah.Controllers;

    public class ReservationManageDto
    {
        public List<SeatDto> Tables { get; set; }

        public List<ReservationDto> Reservations { get; set; }

        public DateTime startTime;

        public DateTime endTime;

        public int TimeSlotSize;

        public DateTime Date;
        
    }
}