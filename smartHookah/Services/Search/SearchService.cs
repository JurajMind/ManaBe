using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using smartHookah.Models.Dto;
using smartHookah.Services.Gear;

namespace smartHookah.Services.Search
{
    public class SearchService : ISearchService
    {
        private readonly SearchIndexClient searchServiceClient;

        public SearchService()
        {
            this.searchServiceClient = CreateSearchServiceClient();
        }

        public async Task<IList<SearchPipeAccessory>> Search(string prefix,string type = null)
        {
            var results = await searchServiceClient.Documents.SearchAsync<SearchPipeAccessory>( $"{prefix}*");
            
            return results.Results.Where(r =>
            {
                if (type == null)
                    return true;
                return String.Equals(r.Document.Type, type, StringComparison.CurrentCultureIgnoreCase);
            }).Select(r => r.Document).ToList();

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
        }


    }
}