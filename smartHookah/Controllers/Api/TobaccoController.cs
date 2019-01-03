using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using smartHookah.Models;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;

namespace smartHookah.Controllers.Api
{
    [System.Web.Http.RoutePrefix("api/Tobacco")]
    public class TobaccoController : ApiController
    {
        private readonly ITobaccoService tobaccoService;

        public TobaccoController(ITobaccoService tobaccoService)
        {
            this.tobaccoService = tobaccoService;
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
                var sessionsTask = tobaccoService.GetTobaccoSessions(tobacco);
                var reviewsTask = tobaccoService.GetTobaccoReviews(tobacco);

                await Task.WhenAll(sessionsTask, reviewsTask);
                
                var sessions = await sessionsTask;
                var reviews = await reviewsTask;

                return TobaccoInformationDto.FromModel(tobacco, tastes, personStats, stats, sessions, reviews);
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

        [HttpGet, Route("{id}/GetReviews")]
        public async Task<List<TobaccoReviewDto>> GetTobaccoReviews(int id, int pageSize = 10, int page = 0)
        {
            try
            {
                var tobacco = await tobaccoService.GetTobacco(id);
                var reviews = await tobaccoService.GetTobaccoReviews(tobacco, pageSize, page);
                return TobaccoReviewDto.FromModelList(reviews).ToList();
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

        #endregion

    }
}
