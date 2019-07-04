using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http;
using Newtonsoft.Json;
using smartHookah.Models.Db;

namespace smartHookah.Models.Dto
{
    [DataContract]
    public class PipeAccesorySimpleDto
    {
        [DataMember]
        [JsonProperty("Id")]
        public int Id { get; set; }
         
        [DataMember]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("Brand")]
        public string BrandName { get; set; }

        [DataMember]
        [JsonProperty("BrandId")]
        public string BrandId { get; set; }

        [DataMember]
        [JsonProperty("Picture")]
        public string Picture { get; set; }

        [DataMember]
        [JsonProperty("Type")]
        public string Type { get; set; }

        [DataMember]
        [JsonProperty("Likes")]
        public List<PipeAccesoryLikeDto> Likes;

        [DataMember]
        [JsonProperty("LikeCount")]
        public int LikeCount { get; set; }

        [DataMember]
        [JsonProperty("DisLikeCount")]
        public int DisLikeCount { get; set; }


        [Authorize(Roles = "Admin")]
        public static PipeAccesorySimpleDto FromModel(PipeAccesory model, bool includeVotes) => model == null
            ? null
            : new PipeAccesorySimpleDto()
            {
                Id = model.Id,
                BrandName = model.Brand.DisplayName,
                BrandId = model.BrandName,
                Picture = model.Picture ?? model.Brand.Picture,
                Name = model.AccName,
                Type = model.GetTypeName(),
                Likes = includeVotes ? GetLikesList(model.Likes) : null,
                LikeCount = model.LikeCount,
                DisLikeCount = model.DisLikeCount
            };
        
        public static PipeAccesorySimpleDto FromModel(PipeAccesory model) => model == null
            ? null
            : new PipeAccesorySimpleDto()
            {
                Id = model.Id,
                BrandName = model.Brand.DisplayName,
                BrandId = model.BrandName,
                Picture = model.Picture ?? model.Brand.Picture,
                Name = model.AccName,
                Type = model.GetTypeName(),
                Likes = null,
                LikeCount = model.LikeCount,
                DisLikeCount = model.DisLikeCount
            };

        public static IEnumerable<PipeAccesorySimpleDto> FromModelList(IEnumerable<PipeAccesory> model)
        {
            if(model == null) yield break;
            foreach (var item in model)
            {
                yield return FromModel(item);
            }
        }

        public static PipeAccesorySimpleDto FromModel(Bowl model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Bowl";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Pipe model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Hookah";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Tobacco model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Tobacco";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(HeatManagment model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "HeatManagement";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Coal model)
        {
            if (model == null) return null;
            var result = FromModel(model as PipeAccesory);
            result.Type = "Coal";
            return result;
        }

        private static List<PipeAccesoryLikeDto> GetLikesList(IEnumerable<PipeAccesoryLike> model)
        {
            return model.Select(PipeAccesoryLikeDto.FromModel).ToList();
        }

        public PipeAccesory ToModel()
        {

            var pipeAccessory = new PipeAccesory()
            {
                BrandName = this.BrandId ?? this.BrandName,
                AccName = this.Name,
                Id = this.Id,

            };

            switch (this.Type)
            {

                case "Hookah":
                    return new Pipe(pipeAccessory);
                case "Bowl":
                    return new Bowl(pipeAccessory);
                case "Tobacco":
                {
                    var protoTobbacco = new Tobacco(pipeAccessory);
                    return protoTobbacco;
                }
                case "HeatManagement":
                {
                    return new HeatManagment(pipeAccessory);
                }
                case "Coal":
                {
                    return new Coal(pipeAccessory);
                }
                default:
                    return pipeAccessory;


            }
        }
    }
    
    public class PipeAccesoryLikeDto
    {
        [DataMember]
        [JsonProperty("Id")]
        public int Id { get; set; }

        [DataMember]
        [JsonProperty("PersonId")]
        public int PersonId { get; set; }

        [DataMember]
        [JsonProperty("PipeAccessoryId")]
        public int PipeAccesoryId { get; set; }

        [DataMember]
        [JsonProperty("Value")]
        public int Value { get; set; }

        public static PipeAccesoryLikeDto FromModel(PipeAccesoryLike model) => model == null
            ? null
            : new PipeAccesoryLikeDto()
            {
                Id = model.Id,
                PersonId = model.PersonId,
                PipeAccesoryId = model.PipeAccesoryId,
                Value = model.Value
            };

        public static PipeAccesorySimpleDto FromModel(HeatManagment model)
        {
            if (model == null) return null;
            var result = FromModel(model);
            result.Type = "HeatManagement";
            return result;
        }

        public static PipeAccesorySimpleDto FromModel(Coal model)
        {
            if (model == null) return null;
            var result = FromModel(model);
            result.Type = "Coal";
            return result;
        }

    }
}