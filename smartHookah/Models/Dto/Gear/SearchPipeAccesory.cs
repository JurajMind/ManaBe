namespace smartHookah.Models.Dto
{
    public class SearchPipeAccessory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Brand { get; set; }

        public string Type { get; set; }

        public bool NonVerified { get; set; }

        public SearchPipeAccessory()
        {

        }

        public SearchPipeAccessory(Services.Search.SearchService.SearchPipeAccessory accessory)
        {
            this.Id = accessory.Id;
            this.Brand = accessory.Brand;
            this.Type = accessory.Type;
            this.Name = accessory.Name;
            this.NonVerified = accessory.Status != 0;
        }
    }

}