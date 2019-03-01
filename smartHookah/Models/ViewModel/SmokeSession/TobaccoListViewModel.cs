using System.Collections.Generic;
using smartHookah.Models.Db;

namespace smartHookah.Models
{
    public class TobaccoListViewModel
    {
        public List<Tobacco> Tobaccos { get; set; }

            public bool CanEdit { get; set; }
    }
}