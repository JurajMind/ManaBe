using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Newtonsoft.Json;
using smartHookah.Services.Person;

namespace smartHookah.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly IPersonService personService;
        private readonly SearchIndexClient searchServiceClient;

        public SearchService(IPersonService personService)
        {
            this.personService = personService;
            this.searchServiceClient = CreateSearchServiceClient();
        }

        public async Task<IList<SearchPipeAccessory>> Search(string prefix,string type = null)
        {
            var personId = this.personService.GetCurentPersonId();
            DocumentSearchResult<SearchPipeAccessory> results;
            if (type == null)
            {
                if (prefix.Contains(" "))
                {
                    var chunks = prefix.Replace(" ", "* ");
                }

                results = await searchServiceClient.Documents.SearchAsync<SearchPipeAccessory>($"{prefix.Trim()}*");
            }
            else
            {
                results = await searchServiceClient.Documents.SearchAsync<SearchPipeAccessory>($"{prefix}*",new SearchParameters(){QueryType = QueryType.Full,Filter = $"Discriminator eq '{type}'" });
            }


            return results.Results.Where(a => a.Document.Status == 0 || a.Document.CreatorId == personId).Select(r => r.Document).ToList();

        }

        public async Task<bool> UpdateIndex()
        {
            return false;
        }


        private static SearchIndexClient CreateSearchServiceClient()
        {
            string searchServiceName = ConfigurationManager.AppSettings["SearchServiceName"];
            string queryApiKey = ConfigurationManager.AppSettings["SearchServiceAdminApiKey"];

            SearchIndexClient serviceClient = new SearchIndexClient(searchServiceName, "azuresql-index", new SearchCredentials(queryApiKey));
            return serviceClient;
        }

        public class SearchPipeAccessory
        {
            [System.ComponentModel.DataAnnotations.Key]
            [IsFilterable]
            public int Id { get; set; }

            [IsSearchable]
            [JsonProperty("DisplayName")]
            public string Brand { get; set; }

            [IsSearchable]
            [JsonProperty("AccName")]
            public string Name { get; set; }

            [JsonProperty("Discriminator")]
            public string Type { get; set; }

            [JsonProperty("Status")]
            public int Status { get; set; }

            [JsonProperty("CreatorId")]
            public int? CreatorId { get; set; }
        }


    }
}