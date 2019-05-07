using System;
using System.Collections.Generic;

namespace smartHookah.Models.Dto.Places.Reservations
{
    public class ReservationManageDto
    {
        public List<SeatDto> Tables { get; set; }

        public List<Places.Reservations.ReservationDto> Reservations { get; set; }

        public DateTime startTime;

        public DateTime endTime;

        public int TimeSlotSize;

        public DateTime Date;
        
    }
}