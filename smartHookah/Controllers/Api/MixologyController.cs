using System;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;

using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;

namespace smartHookah.Controllers.Api
{
    using System.Data.Entity;
    using System.Net;
    using System.Net.Http;

    using DocumentFormat.OpenXml.Office2010.Excel;

    using log4net;

    using MaxMind.GeoIP2.Exceptions;

    using smartHookah.Migrations;
    using smartHookah.Services.Person;

    [System.Web.Http.RoutePrefix("api/Mixology")]
    public class MixologyController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        private readonly IGearService gearService;

        private readonly ILog logger = LogManager.GetLogger(typeof(MixologyController));

        public MixologyController(SmartHookahContext db,IPersonService personService, IGearService gearService)
        {
            this.db = db;
            this.personService = personService;
            this.gearService = gearService;
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
                var user = this.personService.GetCurentPerson();
                if (user == null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden,"User not found"));
                }
                ;
                var userId = user.Id;
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
                                        Tobacco = new TobaccoSimpleDto()
                                                      {
                                                          Id = x.TobaccoId,
                                                          Name = x.Tobacco.AccName,
                                                          BrandName = x.Tobacco.Brand.DisplayName,
                                                          Type = "Tobbaco",
                                                          SubCategory = x.Tobacco.SubCategory
                                                          
                                                      },
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
        public async Task<TobaccoMixSimpleDto> AddToMix([Bind(Include = "Id,AccName,Tobaccos")] Mix newMix)
        {
            if (newMix == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Mix is null"));
            }

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
                var t = this.db.Tobaccos.Find(tobacco.Tobacco.Id);
                if (tobacco.Fraction < 1 || tobacco.Fraction > 40 || t == null)
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        "TobaccoInMix not fount or fraction not within acceptable range."));

                if (mix.Tobaccos.Any(a => a.TobaccoId == tobacco.Tobacco.Id))
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        $"TobaccoInMix {tobacco.Tobacco.BrandName} {tobacco.Tobacco.Name} was already added to mix."));

                mix.Tobaccos.Add(new TobacoMixPart(){TobaccoId = tobacco.Tobacco.Id, Fraction = tobacco.Fraction});
            }
            try
            {
                this.db.TobaccoMixs.AddOrUpdate(mix);
                await this.db.SaveChangesAsync();
                var response = new TobaccoMixSimpleDto()
                {
                    Id = mix.Id,
                    Name = mix.AccName
                };
                foreach (var m in mix.Tobaccos)
                {
                    var x = new TobaccoInMix()
                    {
                        Tobacco =  new TobaccoSimpleDto()
                                       {
                                           Id = m.Tobacco.Id,
                                           Name = m.Tobacco.AccName,
                                           BrandId = m.Tobacco.BrandName,
                                           BrandName = m.Tobacco.Brand.Name
                                       },
                        Fraction = m.Fraction
                    };
                    response.Tobaccos.Add(x);
                }
                return response;
            }
            catch (Exception e)
            {
                this.logger.Error(e);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError,
                    $"Server error"));
            }
        }

        [System.Web.Http.HttpPost, System.Web.Http.Authorize, System.Web.Http.Route("{id}/Vote")]
        public HttpResponseMessage Vote(int id, [FromBody] int value)
        {
            value = value < 0 ? (int)VoteValue.Dislike : value > 0 ? (int)VoteValue.Like : (int)VoteValue.Unlike;
            try
            {
                gearService.Vote(id, (VoteValue)value);
            }
            catch (Exception e)
            {
                var err = new HttpError(e.Message);
                return Request.CreateResponse(HttpStatusCode.NotFound, err);
            }
            return new HttpResponseMessage(HttpStatusCode.OK);
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