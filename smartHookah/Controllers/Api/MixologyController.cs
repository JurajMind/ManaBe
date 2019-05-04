﻿using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.Azure.Amqp.Framing;
using smartHookah.Models;
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

    using DocumentFormat.OpenXml.Office2010.Excel;

    using log4net;

    using MaxMind.GeoIP2.Exceptions;

    using smartHookah.ErrorHandler;
    using smartHookah.Migrations;
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
        public async Task<IEnumerable<TobaccoMixSimpleDto>> GetMixes(int page = 0, int pageSize = 50, string author = "me", string orderBy = "name", string order = "asc")
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
            var result = new List<TobaccoMixSimpleDto>();
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
                    throw new HttpResponseException(this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, $"bad order value {orderBy}"));
            }
            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            return TobaccoMixSimpleDto.FromModelList(res);
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("GetMixCreators")]
        public MixCreatorsDto GetFeaturedMixCreators(int page = 0, int pageSize = 50, string orderBy = "name", string order = "asc")
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
                    query = order.ToLower() == "asc" ? from a in query orderby a.PipeAccessories.Count(x => x is TobaccoMix) ascending select a : from a in query orderby a.PipeAccessories.Count(x => x is TobaccoMix) descending select a;
                    break;
                default:
                    throw new ManaException(ErrorCodes.WrongOrderField, "Invalid OrderBy value, select \"name\" or \"count\".");
            }

            query = pageSize > 0 && page >= 0 ? query.Skip(pageSize * page).Take(pageSize) : query.Take(50);

            var res = query.ToList();

            if (res.Any())
            {
                var result = new MixCreatorsDto();
                foreach (var m in res)
                {
                    var creator = new MixCreator()
                    {
                        Name = m.Name,
                        DisplayName = m.DisplayName,
                        Picture = m.Picture,
                        MixCount = m.PipeAccessories.Count(a => a is TobaccoMix)
                    };

                    result.MixCreatorsList.Add(creator);
                }
                return result;
            }

            return new MixCreatorsDto();
        }

        [System.Web.Http.HttpGet, System.Web.Http.Route("{id}/GetMix")]
        public async Task<TobaccoMixSimpleDto> GetTobaccoMix(int id)
        {
            try
            {
                var mix = await tobaccoService.GetTobaccoMix(id);
                return TobaccoMixSimpleDto.FromModel(mix);
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
            return TobaccoMixSimpleDto.FromModel(result);
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