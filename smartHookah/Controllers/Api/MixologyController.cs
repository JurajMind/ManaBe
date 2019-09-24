using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using smartHookah.Models.Db;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;
using smartHookahCommon.Errors;
using smartHookahCommon.Exceptions;

namespace smartHookah.Controllers.Api
{
    using System.Data.Entity;
    using System.Net;
    using System.Net.Http;
    using log4net;
    using smartHookah.ErrorHandler;
    using smartHookah.Services.Person;

    [System.Web.Http.RoutePrefix("api/Mixology")]
    public class MixologyController : ApiController
    {
        private readonly SmartHookahContext db;

        private readonly IPersonService personService;

        private readonly IGearService gearService;

        private readonly ITobaccoService tobaccoService;

        private readonly ILog logger = LogManager.GetLogger(typeof(MixologyController));

        public MixologyController(SmartHookahContext db,IPersonService personService, IGearService gearService, ITobaccoService tobaccoService)
        {
            this.db = db;
            this.personService = personService;
            this.gearService = gearService;
            this.tobaccoService = tobaccoService;
        }
        
        #region Getters

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixes")]
        [ApiAuthorize]
        public async Task<IEnumerable<TobaccoMixSimpleDto>> GetMixes(int page = 0, int pageSize = 50, string author = "me", string orderBy = "created", string order = "dsc")
        {
           var query = from a in this.db.TobaccoMixs select a;
            if (await this.db.Brands.AnyAsync(a => a.TobaccoMixBrand && a.Name.ToLower() == author.ToLower()))
            {
                query = from m in query where m.Brand.Name.ToLower() == author.ToLower() && m.Deleted == false  select m;
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
                query = from m in query where m.Author.Id == userId &&  m.Deleted == false select m;
            }
            else if(author == "favorite")
            {
                var user = this.personService.GetCurentPerson();
                if (user == null)
                {
                    throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.Forbidden, "User not found"));
                }

                query = user.Likes.Where(a => a.PipeAccesory is TobaccoMix mix && a.Value > 0).Select(c => c.PipeAccesory as TobaccoMix).AsQueryable();

            }
            var result = new List<TobaccoMixSimpleDto>();
            switch (orderBy.ToLower())
            {
                case "name":
                    query = order == "asc"
                                ? from a in query orderby a.AccName ascending select a
                                : from a in query orderby a.AccName descending select a;
                    break;
                case "created":
                    query = order == "asc"
                        ? from a in query orderby a.CreatedAt ascending select a
                        : from a in query orderby a.CreatedAt descending select a;
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
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"bad order value {orderBy}"));
            }
            query = pageSize > 0 && page >= 0 ? query.Where(a => a.Tobaccos.Count > 0).Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();
            var person = this.personService.GetCurentPerson();
            return TobaccoMixSimpleDto.FromModelList(res,person.Id);
        }


        [System.Web.Http.HttpGet, System.Web.Http.Route("{id}/GetMix")]
        public async Task<TobaccoMixSimpleDto> GetTobaccoMix(int id)
        {
            try
            {
                var mix = await tobaccoService.GetTobaccoMix(id);
                return TobaccoMixSimpleDto.FromModel(mix,this.personService.GetCurentPerson()?.Id);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }

        [System.Web.Http.HttpGet, System.Web.Http.Route("{id}/GetTastes")]
        public async Task<Dictionary<int, IEnumerable<TobaccoTasteDto>>> GetTobaccoMixTastes(int id)
        {
            try
            {
                var mix = await tobaccoService.GetTobaccoMix(id);
                var result = tobaccoService.GetTobaccoMixTastes(mix);
                var resultDto = new Dictionary<int, IEnumerable<TobaccoTasteDto>>();
                foreach (var item in result)
                {
                    resultDto.Add(item.Key, TobaccoTasteDto.FromModelList(item.Value));
                }

                return resultDto;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }
        [System.Web.Http.HttpGet, System.Web.Http.Route("Suggest/Mix")]
        public async Task<List<TobaccoMixSimpleDto>> GetMiFromTobaccos([FromUri] int[] ids, int pageSize = 100, int page = 0,
            bool own = true)
        {
            var result = await this.tobaccoService.GetMixFromTobaccos(ids.ToList(), pageSize, page);
            return TobaccoMixSimpleDto.FromModelList(result,this.personService.GetCurentPersonId()).ToList();
        }

        [System.Web.Http.HttpGet, System.Web.Http.Route("Suggest/Tobacco")]
        public async Task<List<TobaccoSimpleDto>> SuggestMixTobacco([FromUri] int[] ids, int pageSize = 100, int page = 0,
            bool own = true)
        {
            var result = await this.tobaccoService.SuggestTobaccos(ids.ToList(), pageSize, page,own);
            return TobaccoSimpleDto.FromModelList(result).ToList();
        }

        #endregion

        #region Setters

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("AddToMix")]
        public async Task<TobaccoMixSimpleDto> AddToMix([Bind(Include = "Id,AccName,Tobaccos")] TobaccoMixSimpleDto newMix)
        {
            if (newMix == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Mix is null"));
            }
            TobaccoMix newMixModel = TobaccoMixSimpleDto.ToModel(newMix);
            var result = await this.tobaccoService.AddOrUpdateMix(newMixModel);
            return TobaccoMixSimpleDto.FromModel(result, this.personService.GetCurentPerson()?.Id);
        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.Route("RenameMix/{id}")]
        public async Task<TobaccoMixSimpleDto> RenameMix(int id, string newName)
        {
            var mix = await this.db.PipeAccesories.FindAsync(id) as TobaccoMix;
            if (mix == null)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound,
                    "Mix is null"));
            }

            var person = this.personService.GetCurentPerson();
            if(mix.AuthorId != person.Id)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Only author can rename mix"));
            }
            mix.AccName = newName;
            this.db.PipeAccesories.AddOrUpdate(mix);
            await this.db.SaveChangesAsync();
            return TobaccoMixSimpleDto.FromModel(mix as TobaccoMix, this.personService.GetCurentPerson()?.Id);
        }

        [System.Web.Http.HttpPost, ApiAuthorize, System.Web.Http.Route("{id}/Vote")]
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
            var person = this.personService.GetCurentPerson();
            if (mix.AuthorId != person.Id)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Only author can remove  mix"));
            }

            try
            {
                mix.Deleted = true;
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