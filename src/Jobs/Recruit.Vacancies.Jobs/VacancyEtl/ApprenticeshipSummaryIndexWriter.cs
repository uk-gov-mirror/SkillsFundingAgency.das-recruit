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
        private const string ApprenticeshipDocumentTypeName = "apprenticeship";
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
                var requestUri = $"{indexName}/{ApprenticeshipDocumentTypeName}/{item.Id}";
                var resp = await _httpClient.PostAsync(requestUri, content);

                if (resp.IsSuccessStatusCode)
                {
                    _logger.LogDebug($"Successfully added vacancy '{item.VacancyReference} {item.Title}' to the '{indexName}' index.");
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"Http error {ex.Message} when trying to add vacancy '{item.VacancyReference} {item.Title}' to the '{indexName}' index.");
            }
            
            return false;
        }

        public async Task<bool> IndexBulkAsync(string indexName, IList<ApprenticeshipSummary> items)
        {
            var actionDocs = items.Select(x => $"{{ \"index\" : {{ \"_index\" : \"{indexName}\", \"_type\" : \"{ApprenticeshipDocumentTypeName}\", \"_id\" : \"{x.Id}\" }} }}\n");
            var docs = items.Select(x => JsonConvert.SerializeObject(x, _jsonSettings) + "\n");
            var req = actionDocs.Zip(docs, (action, indexDoc) => string.Concat(action, indexDoc)).ToList();

            try
            {
                var content = new StringContent(string.Join(string.Empty, req), Encoding.UTF8, RequestMediaType);
                var resp = await _httpClient.PostAsync($"{indexName}/_bulk", content);
                return resp.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"Http error {ex.Message} when trying to bulk insert vacancies to the '{indexName}' index.");
            }

            return false;
        }
    }
}
