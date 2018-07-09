using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace smartHookah.Models.Dto
{
    public class MixCreatorsDTO : DTO
    {
        public ICollection<MixCreator> MixCreatorsList { get; set; }

        public MixCreatorsDTO()
        {
            this.MixCreatorsList = new List<MixCreator>();
        }
    }

    public class MixCreator
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Picture { get; set; }
        public int MixCount { get; set; }
    }
}