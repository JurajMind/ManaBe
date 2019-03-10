using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using smartHookah.Models;
using smartHookah.Models.Db;

namespace smartHookah.Services.Gear
{
    using System.Web.Helpers;

    using smartHookah.Models.Dto;

    public interface IGearService
    {
        List<PipeAccesory> GetPersonAccessories(int? personId, string type);

        PipeAccesory GetPipeAccessory(int id);

        void Vote(int id, VoteValue value);

        Dictionary<AccesoryType, List<BrandGroupDto>> GetBrands();

        List<Models.Dto.GearService.SearchPipeAccesory> SearchAccesories(string search, AccesoryType type, SearchType searchType,int page,int pageSize);
        
        int UsedByPerson(PipeAccesory accessory, int personId);

        int UsedByPerson(PipeAccesory accessory);

        IDictionary<PipeAccesory, int> UsedWith(PipeAccesory accessory);

        int OwnedByPersons(PipeAccesory accessory);

        int OwnedByPlaces(PipeAccesory accessory);

        Task<PipeAccesory> AddMyGear(int id, int count, int? personId);

        Task<bool> DeleteMyGear(int id, int? count, int? personId);

        ICollection<PipeAccesory> GetRecentAccessories(int count);
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