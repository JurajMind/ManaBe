﻿using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models;

namespace smartHookah.Services.Gear
{
    public interface ITobaccoService
    {
        Task<Tobacco> GetTobacco(int id);
        PipeAccesoryStatistics GetTobaccoStatistics(Tobacco tobacco);
        PipeAccesoryStatistics GetPersonTobaccoStatistics(Tobacco tobacco);
        Task<PipeAccesoryStatistics> GetPersonTobaccoStatistics(Tobacco tobacco, int personId);
        List<TobaccoTaste> GetTobaccoTastes(Tobacco tobacco);
        Task<List<Models.SmokeSession>> GetTobaccoSessions(Tobacco tobacco, int pageSize = 10, int page = 0);
        Task<List<TobaccoReview>> GetTobaccoReviews(Tobacco tobacco, int pageSize = 10, int page = 0);


        IList<Tobacco> GetTobaccoList(int page, int pageSize, TobaccoFilter filter);


        Task<TobaccoMix> GetTobaccoMix(int id);
        Task<PipeAccesoryStatistics> GetTobaccoMixStatistics(TobaccoMix mix);
        Task<PipeAccesoryStatistics> GetPersonTobaccoMixStatistics(TobaccoMix mix);
        Task<PipeAccesoryStatistics> GetPersonTobaccoMixStatistics(TobaccoMix mix, int personId);
        Dictionary<int, List<TobaccoTaste>> GetTobaccoMixTastes(TobaccoMix mix);
        Task<List<Models.SmokeSession>> GetTobaccoMixSessions(TobaccoMix mix, int count = 10);
        Task<List<TobaccoReview>> GetTobaccoMixReviews(TobaccoMix mix, int count = 10);

        Task<TobaccoMix> AddOrUpdateMix(TobaccoMix newMix);
    }
}