using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbTrainingProgrammeCategoryRepository : MongoDbCollectionBase, IReadRepository<TrainingProgrammeCategory, string>
    {
        private const string Database = "recruit";
        private const string Collection = "trainingProgrammeCategories";

        public MongoDbTrainingProgrammeCategoryRepository(ILogger<MongoDbTrainingProgrammeCategoryRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, Database, Collection, details)
        {
        }

        public async Task<IEnumerable<TrainingProgrammeCategory>> GetAllAsync()
        {
            var collection = GetCollection<TrainingProgrammeCategory>();
            var result = await collection.FindAsync(FilterDefinition<TrainingProgrammeCategory>.Empty);
            return result.ToList();
        }

        public async Task<TrainingProgrammeCategory> GetAsync(string id)
        {
            var filter = Builders<TrainingProgrammeCategory>.Filter.Eq(v => v.Id, id);
            var collection = GetCollection<TrainingProgrammeCategory>();
            var result = await collection.FindAsync(filter);
            return result.SingleOrDefault();
        }
    }
}
