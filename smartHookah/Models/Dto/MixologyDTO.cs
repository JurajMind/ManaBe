﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using DocumentFormat.OpenXml.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace smartHookah.Models.Dto
{
    using Accord.Statistics;

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

    [DataContract]
    public class TobaccoMixSimpleDto : TobaccoSimpleDto
    {
        [DataMember, JsonProperty("Tobaccos")]
        public ICollection<TobaccoInMix> Tobaccos { get; set; }

        public TobaccoMixSimpleDto()
        {
            this.Tobaccos = new List<TobaccoInMix>();
        }

        public static TobaccoMixSimpleDto FromModel(TobaccoMix model)
        {
            return model == null
                ? null
                : new TobaccoMixSimpleDto
                {
                    Name = model.AccName,
                    Id = model.Id,
                    BrandName = model.Brand.Name,
                    BrandId = model.BrandName,
                    Type = "TobaccoMix",
                    Tobaccos = model.Tobaccos.Select(
                        a => new TobaccoInMix()
                        {
                            Fraction = a.Fraction,
                            Tobacco = FromModel(a.Tobacco)
                        }).ToList()
                };
        }
    }

    public class Mix
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public ICollection<TobaccoInMix> Tobaccos { get; set; }
        public int Used { get; set; }
        public double Overall { get; set; }

        public Mix()
        {
            this.Tobaccos = new List<TobaccoInMix>();
        }

        public static Mix FromModel(TobaccoMix model)
        {
            if (model == null)
            {
                return null;
            }

            var result = new Mix()
            {
                Id = model.Id,
                AccName = model.AccName,
                Tobaccos = model.Tobaccos.Select(TobaccoInMix.FromModel).ToList()
            };

            return result;
        }
    }

    public class TobaccoInMix
    {
        public TobaccoInMix()
        {
            Tobacco = new TobaccoSimpleDto();
        }

        public TobaccoSimpleDto Tobacco { get; set; }
        public int Fraction { get; set; }

        public static TobaccoInMix FromModel(TobacoMixPart tobacoMixPart)
        {
            return new TobaccoInMix()
            {
                Fraction = tobacoMixPart.Fraction,
                Tobacco = TobaccoSimpleDto.FromModel(tobacoMixPart.Tobacco)
            };
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