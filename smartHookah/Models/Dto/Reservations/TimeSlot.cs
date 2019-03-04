using System.Collections.Generic;

namespace smartHookah.Models.Dto.Reservations
{
    public class TimeSlot
    {
        public int Value { get; set; }
        public string Text { get; set; }
        public bool Reserved { get; set; }

        public int CapacityLeft { get; set; }

        public int MaxTable { get; set; }

        public int Id { get; set; }

        public int OrderIndex { get; set; }

        public TimeSlot()
        {
            
        }

        public TimeSlot(TableTimeSlot table)
        {
            this.Id = table.Id;
            this.Text = table.Text;
            this.Value = table.Value;
            this.OrderIndex = table.OrderIndex;
            this.MaxTable = table.MaxTable;
            this.CapacityLeft = table.CapacityLeft;
        }

    }

    public class TableTimeSlot : TimeSlot
    {
        public TableTimeSlot(TimeSlot timeSlot)
        {
          this.Id = timeSlot.Id;
         this.Text = timeSlot.Text;
         this.Value = timeSlot.Value;
         this.OrderIndex = timeSlot.OrderIndex;
                        TableSlots = new Dictionary<int, TableSlot>();
        }
        public Dictionary<int, TableSlot> TableSlots;
    }

    public class TableSlot
    {

        public int? ReservationId { get; set; }

        public int TableId { get; set; }

        public int Capacity { get; set; }

        public int Used { get; set; }
    }
}
