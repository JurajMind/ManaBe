using System.Collections.Generic;
using System.Threading.Tasks;
using smartHookah.Models.Dto;

namespace smartHookah.Services.Search
{
    public interface ISearchService
    {
        Task<IList<SearchService.SearchPipeAccessory>> Search(string prefix, string type);

        Task<bool> UpdateIndex();
    }
}