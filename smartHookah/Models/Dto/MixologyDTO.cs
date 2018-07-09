﻿using System;
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

    public class MixListDTO : DTO
    {
        public ICollection<Mix> Mixes { get; set; }

        public MixListDTO()
        {
            this.Mixes = new List<Mix>();
        }
    }

    public class Mix
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public ICollection<Tobacco> Tobaccos { get; set; }
        public int Used { get; set; }
        public double Overall { get; set; }

        public Mix()
        {
            this.Tobaccos = new List<Tobacco>();
        }
    }

    public class Tobacco
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public string BrandName { get; set; }
        public int Fraction { get; set; }
    }

    public class MixCreator
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Picture { get; set; }
        public int MixCount { get; set; }
    }
}