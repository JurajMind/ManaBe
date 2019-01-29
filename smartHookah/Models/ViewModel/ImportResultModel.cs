namespace smartHookah.Models.ViewModel
{
    using System.Collections.Generic;

    using smartHookah.Models.Db;

    public class ImportResultModel
    {
        public ImportResultModel()
        {
            this.newBrands = new List<Brand>();
            this.resultTobacco = new List<Tobacco>();
            this.updatedTobacco = new List<PipeAccesory>();
        }
        public  List<Brand> newBrands { get; set; }

        public List<Tobacco> resultTobacco { get; set; }

        public List<PipeAccesory> updatedTobacco { get; set; }
    }
}