using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

using smartHookah.Models;
using smartHookah.Models.Dto;

namespace smartHookah.Controllers.Api
{
    using System.Data.Entity;

    using smartHookah.Services.Person;

    [System.Web.Http.RoutePrefix("api/Mixology")]
    public class MixologyController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        public MixologyController(SmartHookahContext db,IPersonService personService)
        {
            this.db = db;
            this.personService = personService;
        }
        
        #region Getters

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixes")]
        public async Task<MixListDTO> GetMixes(int page = 0, int pageSize = 50, string author = "me", string orderBy = "name", string order = "asc")
        {
            var query = from a in this.db.TobaccoMixs select a;
            if (await this.db.Brands.AnyAsync(a => a.TobaccoMixBrand && a.Name.ToLower() == author.ToLower()))
            {
                query = from m in query where m.Brand.Name.ToLower() == author.ToLower() select m;
            }
            else if (author == "me")
            {
                var userId = this.personService.GetCurentPerson().Id;
                query = from m in query where m.Author.Id == userId select m;
            }

            switch (orderBy.ToLower())
            {
                case "name":
                    query = order == "asc"
                                ? from a in query orderby a.AccName ascending select a
                                : from a in query orderby a.AccName descending select a;
                    break;
                case "used":
                    query = order == "asc"
                                ? from a in query orderby a.Statistics.Used ascending select a
                                : from a in query orderby a.Statistics.Used descending select a;
                    break;
                case "rating":
                    query = order == "asc"
                                ? from a in query orderby a.Statistics.Overall ascending select a
                                : from a in query orderby a.Statistics.Overall descending select a;
                    break;
                case "time":
                    query = order == "asc"
                                ? from a in query orderby a.Statistics.SmokeDurationTick ascending select a
                                : from a in query orderby a.Statistics.SmokeDurationTick descending select a;
                    break;
                default:
                    return new MixListDTO()
                               {
                                   Success = false,
                                   Message =
                                       "Invalid OrderBy value, select \"name\", \"used\", \"rating\" or \"time\"."
                               };
            }
            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            if (res.Count > 0)
            {
                var result = new MixListDTO() { Success = true, Message = $"{res.Count} mixes found." };
                foreach (var r in res)
                {
                    var m = new Mix()
                                {
                                    Id = r.Id,
                                    AccName = r.AccName,
                                    Overall = r.Statistics?.Overall ?? -1,
                                    Used = r.Statistics?.Used ?? -1
                                };
                    foreach (var x in r.Tobaccos)
                    {
                        var t = new Models.Dto.TobaccoInMix()
                                    {
                                        Id = x.TobaccoId,
                                        AccName = x.Tobacco.AccName,
                                        BrandName = x.Tobacco.BrandName,
                                        Fraction = x.Fraction
                                    };

                        m.Tobaccos.Add(t);
                    }
                    result.Mixes.Add(m);
                }
                return result;
            }
            return new MixListDTO() { Success = false, Message = "No mixes found." };
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixCreators")]
        public MixCreatorsDTO GetFeaturedMixCreators(int page = 0, int pageSize = 50, string orderBy = "name", string order = "asc")
        {
            var query = from b in this.db.Brands
                        where b.TobaccoMixBrand
                        select b;
            
            switch (orderBy.ToLower())
            {
                case "name":
                    query = order.ToLower() == "asc" ? from a in query orderby a.DisplayName ascending select a : from a in query orderby a.DisplayName descending select a;
                    break;
                case "count":
                    query = order.ToLower() == "asc" ? from a in query orderby a.PipeAccesories.Count(x => x is TobaccoMix) ascending select a : from a in query orderby a.PipeAccesories.Count(x => x is TobaccoMix) descending select a;
                    break;
                default:
                    return new MixCreatorsDTO() { Success = false, Message = "Invalid OrderBy value, select \"name\" or \"count\"." };
            }

            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            if (res.Any())
            {
                var result = new MixCreatorsDTO() { Success = true, Message = $"{res.Count()} mix creators found." };
                foreach (var m in res)
                {
                    var creator = new MixCreator()
                    {
                        Name = m.Name,
                        DisplayName = m.DisplayName,
                        Picture = m.Picture,
                        MixCount = m.PipeAccesories.Count(a => a is TobaccoMix)
                    };

                    result.MixCreatorsList.Add(creator);
                }
                return result;
            }

            return new MixCreatorsDTO() { Success = false, Message = "No mix creators found." };
        }
        #endregion

        #region Setters

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AddToMix")]
        public async Task<TobaccoMixDTO> AddToMix([Bind(Include = "Id,AccName,Tobaccos")] Mix newMix)
        {
            if (newMix == null) return new TobaccoMixDTO() { Success = false, Message = "Mix is null." };

            var author = this.personService.GetCurentPerson();
            var mix = new TobaccoMix()
            {
                AccName = newMix.AccName,
                Author = author,
                CreatedAt = DateTimeOffset.UtcNow,
                BrandName = author.AssignedBrandId ?? "OwnBrand"
            };

            foreach (var tobacco in newMix.Tobaccos)
            {
                var t = this.db.Tobaccos.Find(tobacco.Id);
                if (tobacco.Fraction < 1 || tobacco.Fraction > 40 || t == null)
                    return new TobaccoMixDTO() { Success = false, Message = "TobaccoInMix not fount or fraction not within acceptable range." };
                
                if (mix.Tobaccos.Any(a => a.TobaccoId == tobacco.Id))
                    return new TobaccoMixDTO() { Success = false, Message = $"TobaccoInMix {tobacco.BrandName} {tobacco.AccName} was already added to mix." };

                mix.Tobaccos.Add(new TobacoMixPart(){TobaccoId = tobacco.Id, Fraction = tobacco.Fraction});
            }
            try
            {
                this.db.TobaccoMixs.AddOrUpdate(mix);
                await this.db.SaveChangesAsync();
                var response = new TobaccoMixDTO()
                {
                    Success = true,
                    Message = "TobaccoInMix mix was saved.",
                    Id = mix.Id,
                    AccName = mix.AccName
                };
                foreach (var m in mix.Tobaccos)
                {
                    var x = new TobaccoInMix()
                    {
                        Id = m.Tobacco.Id,
                        Fraction = m.Fraction,
                        AccName = m.Tobacco.AccName,
                        BrandName = m.Tobacco.BrandName
                    };
                    response.Tobaccos.Add(x);
                }
                return response;
            }
            catch (Exception e)
            {
                return new TobaccoMixDTO(){Success = false, Message = e.Message};
            }
        }

        #endregion

        #region Deleters

        [System.Web.Http.HttpDelete]
        [System.Web.Http.Route("RemoveMix")]
        public async Task<DTO> RemoveMix(int mixId)
        {
            var mix = this.db.TobaccoMixs.Find(mixId);
            if(mix == null) return new DTO(){ Success = false, Message = $"Mix with id {mixId} not found." };
            try
            {
                if (mix.Statistics != null && mix.Statistics.Used > 0)
                {
                    mix.Author = null;
                    this.db.TobaccoMixs.AddOrUpdate(mix);
                    this.db.SaveChanges();
                    return new DTO(){ Success = true, Message = $"Author of mix {mix.Id} removed." };
                }
                this.db.TobaccoMixs.Remove(mix);
                this.db.SaveChanges();
                return new DTO() { Success = true, Message = $"Mix {mix.Id} removed." };
            }
            catch (Exception e)
            {
                return new DTO(){Success = false, Message = e.Message};
            }
        }

        #endregion
    }
}