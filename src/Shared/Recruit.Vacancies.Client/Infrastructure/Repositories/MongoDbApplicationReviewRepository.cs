﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using MongoDB.Bson;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    internal sealed class MongoDbApplicationReviewRepository : MongoDbCollectionBase, IApplicationReviewRepository
    {
        private const string EmployerAccountId = "employerAccountId";
        private const string VacancyReference = "vacancyReference";
        private const string Id = "_id";

        public MongoDbApplicationReviewRepository(ILogger<MongoDbApplicationReviewRepository> logger, IOptions<MongoDbConnectionDetails> details)
            : base(logger, MongoDbNames.RecruitDb, MongoDbCollectionNames.ApplicationReviews, details)
        {
        }

        public Task CreateAsync(ApplicationReview review)
        {
            var collection = GetCollection<ApplicationReview>();
            return RetryPolicy.ExecuteAsync(context => collection.InsertOneAsync(review), new Context(nameof(CreateAsync)));
        }

        public async Task<ApplicationReview> GetAsync(Guid applicationReviewId)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReviewId);
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter),
                new Context(nameof(GetAsync)));

            return result.SingleOrDefault();
        }

        public async Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId)
        {
            var builder = Builders<ApplicationReview>.Filter;
            var filter = builder.Eq(r => r.VacancyReference, vacancyReference) &
                         builder.Eq(r => r.CandidateId, candidateId);
            
            var collection = GetCollection<ApplicationReview>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync(filter),
                new Context(nameof(GetAsync)));

            return result.SingleOrDefault();
        }

        public Task UpdateAsync(ApplicationReview applicationReview)
        {
            var filter = Builders<ApplicationReview>.Filter.Eq(r => r.Id, applicationReview.Id);
            var collection = GetCollection<ApplicationReview>();

            return RetryPolicy.ExecuteAsync(context => collection.ReplaceOneAsync(filter, applicationReview),
                new Context(nameof(UpdateAsync)));
        }

        public async Task<List<T>> GetForEmployerAsync<T>(string employerAccountId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(EmployerAccountId, employerAccountId);
            var collection = GetCollection<BsonDocument>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync<T>(filter),
                new Context(nameof(GetForEmployerAsync)));

            return await result.ToListAsync();
        }

        public async Task<List<T>> GetForVacancyAsync<T>(long vacancyReference)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(VacancyReference, vacancyReference);
            var collection = GetCollection<BsonDocument>();

            var result = await RetryPolicy.ExecuteAsync(context => collection.FindAsync<T>(filter),
                new Context(nameof(GetForVacancyAsync)));

            return await result.ToListAsync();
        }
    }
}