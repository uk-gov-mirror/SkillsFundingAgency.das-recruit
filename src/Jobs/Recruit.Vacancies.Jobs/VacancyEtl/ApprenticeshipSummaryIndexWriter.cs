using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class ApprenticeshipSummaryIndexWriter : IIndexWriter<ApprenticeshipSummary>
    {
        private readonly ILogger<ApprenticeshipSummaryIndexWriter> _logger;
        private readonly HttpClient _httpClient;
        private const string RequestMediaType = "application/json";
        private static JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public ApprenticeshipSummaryIndexWriter(ILogger<ApprenticeshipSummaryIndexWriter> logger, string elasticSearchHost)
        {
            _logger = logger;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(elasticSearchHost),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        public async Task<bool> IndexAsync(string indexName, ApprenticeshipSummary item)
        {
            var searchDocument = JsonConvert.SerializeObject(item, _jsonSettings);
            var content = new StringContent(searchDocument, Encoding.UTF8, RequestMediaType);

            try
            {
                var resp = await _httpClient.PostAsync($"{indexName}/apprenticeship/{item.Id}", content);

                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Successfully added vacancy {item.VacancyReference} {item.Title} to the {indexName} index.");
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"Http error {ex.Message} when trying to add vacancy {item.VacancyReference} {item.Title} to the {indexName} index.");
            }
            
            return false;
        }

        //public async Task<bool> IndexBulkAsync(string indexName, IList<ApprenticeshipSummary> items)
        //{
        //    var docs = items.Select(x => JsonConvert.SerializeObject(x, _jsonSettings) + Environment.NewLine);
            

        //    //docs.Zip


        //    //batch size control

        //    var content = new StringContent(searchDocument, Encoding.UTF8, "application/json");
        //    var res = await _httpClient.PostAsync(indexName, content);

        //    if (res.IsSuccessStatusCode)
        //    {
        //        // debug log
        //        return true;
        //    }

        //    // warning 
        //    return false;
        //}
    }
}
