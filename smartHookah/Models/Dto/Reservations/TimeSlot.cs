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
    }


    }
