﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbVacancyReviewRepository : MongoDbCollectionBase, IVacancyReviewRepository
    {
        private const string Database = "recruit";
        private const string Collection = "vacancyReviews";

        public MongoDbVacancyReviewRepository(ILogger<MongoDbVacancyReviewRepository> logger, IOptions<MongoDbConnectionDetails> details) 
            : base(logger, Database, Collection, details)
        {
        }

        public Task CreateAsync(VacancyReview vacancy)
        {
            var collection = GetCollection<VacancyReview>();
            return RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(vacancy), new Context(nameof(CreateAsync)));
        }

        public async Task<VacancyReview> GetAsync(Guid reviewId)
        {
            var filter = Builders<VacancyReview>.Filter.Eq(r => r.Id, reviewId);

            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter), new Context(nameof(GetAsync)));
            return result.SingleOrDefault();
        }

        public async Task<IEnumerable<VacancyReview>> GetAllAsync()
        {
            var collection = GetCollection<VacancyReview>();
            var result = await RetryPolicy.ExecuteAsync(context => collection
                                    .Find(FilterDefinition<VacancyReview>.Empty)
                                    .ToListAsync(), new Context(nameof(GetAllAsync)));

            return result.OrderByDescending(x => x.CreatedDate);
        }

        public Task UpdateAsync(VacancyReview review)
        {
            var filterBuilder = Builders<VacancyReview>.Filter;
            var filter = filterBuilder.Eq(v => v.Id, review.Id) & filterBuilder.Eq(v => v.VacancyReference, review.VacancyReference);
            var collection = GetCollection<VacancyReview>();
           
            return RetryPolicy.ExecuteAsync(context => collection.ReplaceOneAsync(filter, review), new Context(nameof(UpdateAsync)));
        }
    }
}
