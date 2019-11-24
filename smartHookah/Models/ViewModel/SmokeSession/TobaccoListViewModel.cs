using smartHookah.Models.Db;
using System.Collections.Generic;

namespace smartHookah.Models
{
    public class TobaccoListViewModel
    {
        public List<Tobacco> Tobaccos { get; set; }

        public bool CanEdit { get; set; }
    }
}