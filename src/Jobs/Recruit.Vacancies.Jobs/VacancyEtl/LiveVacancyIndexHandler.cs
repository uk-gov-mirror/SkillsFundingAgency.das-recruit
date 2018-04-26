using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using System;

namespace Esfa.Recruit.Vacancies.Jobs.VacancyEtl
{
    public class LiveVacancyIndexHandler
    {
        private readonly ILogger<LiveVacancyIndexHandler> _logger;
        private readonly IJobsVacancyClient _client;
        private readonly IIndexWriter<ApprenticeshipSummary> _indexWriter;

        public LiveVacancyIndexHandler(ILogger<LiveVacancyIndexHandler> logger, IJobsVacancyClient client, IIndexWriter<ApprenticeshipSummary> indexWriter)
        {
            _logger = logger;
            _client = client;
            _indexWriter = indexWriter;
        }

        public async Task Handle(string indexName)
        {
            var liveVacanciesTask = _client.GetLiveVacancies();
            var programmesTask = _client.GetActiveApprenticeshipProgrammesAsync();
            var categoriesTask = _client.GetTrainingProgrammeCategories();

            Task.WaitAll(liveVacanciesTask, programmesTask, categoriesTask);
            var programmes = programmesTask.Result;
            var categories = categoriesTask.Result;

            var summaries = liveVacanciesTask.Result.Select(v => ApprenticeshipSummaryMapper.MapFrom(v, programmes, categories))
                                                    .ToList();

            _logger.LogInformation($"{summaries.Count} live vacancies to add to {indexName} index.");
            
            var indexTasks = summaries.Select(x => _indexWriter.IndexAsync(indexName, x));
            Task.WaitAll(indexTasks.ToArray());

            bool failedTasksCheck(Task<bool> t) => t.IsFaulted || t.Result == false;

            if (indexTasks.Any(failedTasksCheck))
            {
                _logger.LogError($"{indexTasks.Count(failedTasksCheck)} vacancies failed to index.");
            }
            else
            {
                _logger.LogInformation($"Successfully indexed {summaries.Count} vacancies.");
            }

            await Task.CompletedTask;
        }
    }
}