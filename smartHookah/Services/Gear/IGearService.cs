﻿using smartHookah.Models.Db;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace smartHookah.Services.Gear
{
    using smartHookah.Models.Dto;
    using System.Web.Helpers;

    public interface IGearService
    {
        List<PipeAccesory> GetPersonAccessories(int? personId, string type);

        PipeAccesory GetPipeAccessory(int id);

        void Vote(int id, VoteValue value);

        Dictionary<AccesoryType, List<BrandGroupDto>> GetBrands();

        List<SearchPipeAccessory> SearchAccessories(string search, AccesoryType type, SearchType searchType, int page, int pageSize);

        int UsedByPerson(PipeAccesory accessory, int personId);

        int UsedByPerson(PipeAccesory accessory);

        IDictionary<PipeAccesory, int> UsedWith(PipeAccesory accessory);

        int OwnedByPersons(PipeAccesory accessory);

        int OwnedByPlaces(PipeAccesory accessory);

        Task<PipeAccesory> AddMyGear(int id, int count, int? personId);

        Task<bool> DeleteMyGear(int id, int? count, int? personId);

        ICollection<PipeAccesory> GetRecentAccessories(int count);

        Task<decimal> TobaccoEstimate(int sessionId);

        ICollection<Models.Db.SmokeSession> UsedInSession(int accessoryId, int pageSize, int page);

        Task<Brand> GetBrand(string brandName);

        Task<PipeAccesory> AddGear(PipeAccesory accessory);

        Task<PipeAccesory> MergeGear(int targetId, int sourceId);


    }

    public class GearFilter
    {
        public string Brand { get; set; }

        public bool Owned { get; set; }

        public bool Smoked { get; set; }

        public SortDirection SortDirection { get; set; }

    }

    public class TobaccoFilter : GearFilter
    {
        private bool Dark { get; set; }
        public List<int> Tastes { get; set; }

        public TobaccoSortBy SortBy { get; set; }

    }

    public enum TobaccoSortBy
    {
        Name,
        Brand,
        SmokeDuration,
        Review,
        Smart,
    }
}