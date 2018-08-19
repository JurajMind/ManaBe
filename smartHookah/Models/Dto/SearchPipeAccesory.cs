namespace smartHookah.Models.Dto
{
    public partial class GearService
    {
        public class SearchPipeAccesory
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public string Brand { get; set; }

            public string Type { get; set; }

            public int Owned { get; set; }
        }
        
    }

}