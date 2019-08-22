using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Models.Dto.Gear;
using smartHookah.Services.Gear;
using smartHookah.Services.Person;
using smartHookah.Services.Review;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/Tobacco")]
    public class TobaccoController : ApiController
    {
        private readonly ITobaccoService tobaccoService;
        private readonly IReviewService reviewService;
        private readonly IPersonService personService;

        public TobaccoController(ITobaccoService tobaccoService,IReviewService reviewService, IPersonService personService)
        {
            this.tobaccoService = tobaccoService;
            this.reviewService = reviewService;
            this.personService = personService;
        }

        #region Getters

        [HttpGet, Route("{id}/GetTobacco")]
        public async Task<TobaccoSimpleDto> GetTobacco(int id)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                return TobaccoSimpleDto.FromModel(tobacco);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpGet, Route("search")]
        public List<TobaccoDto> Search(int page, int pageSize, TobaccoFilter filter)
        {
            var tobaccos = this.tobaccoService.GetTobaccoList(page, pageSize, filter);

             var result = tobaccos.Select(TobaccoDto.FromModel).ToList();
            return result;
        }


        [HttpGet, Route("{id}/GetAllInfo")]
        public async Task<TobaccoInformationDto> GetTobaccoInfo(int id)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                var stats = tobaccoService.GetTobaccoStatistics(tobacco);
                var personStats = tobaccoService.GetPersonTobaccoStatistics(tobacco);
                var tastes = tobaccoService.GetTobaccoTastes(tobacco);
                var sessionsTask =  tobaccoService.GetTobaccoSessions(tobacco);
                var reviewsTask =  this.reviewService.GetTobaccoReviews(tobacco.Id);
                                
                var sessions = await sessionsTask;
                var reviews = await reviewsTask;

                return TobaccoInformationDto.FromModel(tobacco, tastes, personStats, stats, sessions, reviews.ToList());
            }
            catch(Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }

        [HttpGet, Route("{id}/GetTaste")]
        public async Task<List<TobaccoTasteDto>> GetTobaccoTaste(int id)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                var tastes = tobaccoService.GetTobaccoTastes(tobacco);
                var result = new List<TobaccoTasteDto>();

                foreach (var taste in tastes)
                    result.Add(TobaccoTasteDto.FromModel(taste));

                return result;
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }

        [HttpGet, Route("{id}/GetStatistics")]
        public async Task<PipeAccessoryStatisticsDto> GetTobaccoSatistics(int id)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                return PipeAccessoryStatisticsDto.FromModel(tobacco.Statistics);
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }


        [HttpGet, Route("{id}/GetSessions")]
        public async Task<List<SmokeSessionSimpleDto>> GetTobaccoSessions(int id, int pageSize = 10, int page = 0)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                var sessions = await tobaccoService.GetTobaccoSessions(tobacco, pageSize, page);
                return SmokeSessionSimpleDto.FromModelList(sessions).ToList();
            }
            catch (Exception e)
            {
                throw new HttpResponseException(
                    this.Request.CreateErrorResponse(HttpStatusCode.NotFound, e.Message));
            }
        }


        [HttpGet, Route("{id}/InMix")]
        public async Task<List<TobaccoMixSimpleDto>> GetTobaccoInMixes(int id, int pageSize = 10, int page = 0)
        {
            var mixes = await this.tobaccoService.GetMixFromTobacco(id, pageSize, page);

            var result = TobaccoMixSimpleDto.FromModelList(mixes, this.personService.GetCurentPersonId());
            return result.ToList();
        }

        #endregion

    }
}
