﻿namespace smartHookah.Models.ViewModel
{
    using smartHookah.Models.Db;
    using System.Collections.Generic;

    public class ImportResultModel
    {
        public ImportResultModel()
        {
            this.newBrands = new List<Brand>();
            this.newImport = new List<PipeAccesory>();
            this.updateImport = new List<PipeAccesory>();
        }
        public List<Brand> newBrands { get; set; }

        public List<PipeAccesory> newImport { get; set; }

        public List<PipeAccesory> updateImport { get; set; }
    }
}