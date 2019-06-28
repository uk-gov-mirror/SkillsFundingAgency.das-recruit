﻿using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Aspects;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.CommandHandlers;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Events;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Queues;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services.NextVacancyReview;
using Esfa.Recruit.Vacancies.Client.Application.Services.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Application.Services.Reports;
using Esfa.Recruit.Vacancies.Client.Application.Services.VacancyComparer;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Configuration;
using Esfa.Recruit.Vacancies.Client.Infrastructure.EventStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Messaging;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Mongo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BankHolidays;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BannedPhrases;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.BlockedEmployers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Qualifications;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Reports;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.SequenceStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Esfa.Recruit.Vacancies.Client.Infrastructure.StorageQueue;
using Esfa.Recruit.Vacancies.Client.Infrastructure.TableStore;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancyTitle;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.Providers.Api.Client;
using VacancyRuleSet = Esfa.Recruit.Vacancies.Client.Application.Rules.VacancyRules.VacancyRuleSet;

namespace Esfa.Recruit.Vacancies.Client.Ioc
{
    public static class ServiceCollectionExtensions
    {
        public static void AddRecruitStorageClient(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddHttpClient()
                .Configure<AccountApiConfiguration>(configuration.GetSection("AccountApiConfiguration"))
                .AddMemoryCache()
                .AddTransient<IConfigurationReader, ConfigurationReader>()
                .AddSingleton(x =>
                {
                    var svc = x.GetService<IConfigurationReader>();
                    return svc.GetAsync<QaRulesConfiguration>("QaRules").Result;
                });
            RegisterClients(services);
            RegisterServiceDeps(services, configuration);
            RegisterAccountApiClientDeps(services);
            RegisterProviderApiClientDep(services, configuration);
            RegisterTableStorageProviderDeps(services, configuration);
            RegisterRepositories(services, configuration);
            RegisterStorageProviderDeps(services, configuration);
            RegisterQueues(services);
            AddValidation(services);
            AddRules(services);
            RegisterMediatR(services);
            RegisterProviderRelationshipsClient(services, configuration);     
        }

        private static void RegisterProviderRelationshipsClient(IServiceCollection services, IConfiguration configuration)
        {            
            services.Configure<ProviderRelationshipApiConfiguration>(configuration.GetSection("ProviderRelationshipsApiConfiguration"));
            services.AddTransient<IProviderRelationshipsService, ProviderRelationshipsService>();
        }

        private static void RegisterAccountApiClientDeps(IServiceCollection services)
        {
            services.AddSingleton<IAccountApiConfiguration>(kernal => kernal.GetService<IOptions<AccountApiConfiguration>>().Value);
            services.AddTransient<IAccountApiClient, AccountApiClient>();
        }

        private static void RegisterServiceDeps(IServiceCollection services, IConfiguration configuration)
        {
            // Configuration
            services.Configure<GeocodeConfiguration>(configuration.GetSection("Geocode"));
            services.Configure<BankHolidayConfiguration>(configuration.GetSection("BankHoliday"));
            services.Configure<FaaConfiguration>(configuration.GetSection("FaaConfiguration"));
            services.Configure<VacancyApiConfiguration>(configuration.GetSection("VacancyApiConfiguration"));
            services.Configure<SlackConfiguration>(configuration.GetSection("Slack"));
            services.Configure<NextVacancyReviewServiceConfiguration>(o => o.VacancyReviewAssignationTimeoutMinutes = configuration.GetValue<int>("VacancyReviewAssignationTimeoutMinutes"));

            // Domain services
            services.AddTransient<ITimeProvider, CurrentUtcTimeProvider>();

            // Application Service
            services.AddTransient<IGenerateVacancyNumbers, MongoSequenceStore>();
            services.AddTransient<ISlaService, SlaService>();
            services.AddTransient<INotifyVacancyReviewUpdates, SlackNotifyVacancyReviewUpdates>();
            services.AddTransient<INotifyVacancyUpdates, SlackNotifyVacancyUpdates>();
            services.AddTransient<IVacancyService, VacancyService>();
            services.AddTransient<INextVacancyReviewService, NextVacancyReviewService>();
            services.AddTransient<IVacancyComparerService, VacancyComparerService>();
            services.AddTransient<IGetTitlePopularity, TitlePopularityService>();
            services.AddTransient<ICache, Cache>();
            services.AddTransient<IHtmlSanitizerService, HtmlSanitizerService>();
            services.AddTransient<IEmployerService, EmployerService>();

            //Reporting Service
            services.AddTransient<ICsvBuilder, CsvBuilder>();
            services.AddTransient<IReportService, ReportService>();
            services.AddTransient<ProviderApplicationsReportStrategy>();
            services.AddTransient<Func<ReportType, IReportStrategy>>(serviceProvider => reportType =>
            {
                switch (reportType)
                {
                    case ReportType.ProviderApplications:
                        return serviceProvider.GetService<ProviderApplicationsReportStrategy>();
                    default:
                        throw new Exception($"No report strategy for {reportType}");
                }
            });

            // Infrastructure Services
            services.AddTransient<IEmployerAccountProvider, EmployerAccountProvider>();
            services.AddTransient<ISlackClient, SlackClient>();
            services.AddTransient<IGeocodeServiceFactory, GeocodeServiceFactory>();
            services.AddTransient<IGetVacancyTitlesProvider, VacancyApiTitlesProvider>();

            // Projection services
            services.AddTransient<IEmployerDashboardProjectionService, EmployerDashboardProjectionService>();
            services.AddTransient<IProviderDashboardProjectionService, ProviderDashboardProjectionService>();
            services.AddTransient<IQaDashboardProjectionService, QaDashboardProjectionService>();
            services.AddTransient<IEditVacancyInfoProjectionService, EditVacancyInfoProjectionService>();
            services.AddTransient<IPublishedVacancyProjectionService, PublishedVacancyProjectionService>();
            services.AddTransient<IVacancyApplicationsProjectionService, VacancyApplicationsProjectionService>();

            // Reference Data Providers
            services.AddTransient<IMinimumWageProvider, NationalMinimumWageProvider>();
            services.AddTransient<IApprenticeshipProgrammeProvider, ApprenticeshipProgrammeProvider>();
            services.AddTransient<IQualificationsProvider, QualificationsProvider>();
            services.AddTransient<ICandidateSkillsProvider, CandidateSkillsProvider>();
            services.AddTransient<IProfanityListProvider, ProfanityListProvider>();
            services.AddTransient<IBannedPhrasesProvider, BannedPhrasesProvider>();
            services.AddTransient<IBlockedEmployersProvider, BlockedEmployersProvider>();
            services.AddTransient<ITrainingProviderService, TrainingProviderService>();

            // Query Data Providers
            services.AddTransient<IVacancySummariesProvider, VacancySummariesProvider>();

            // Reference Data update services
            services.AddTransient<IApprenticeshipProgrammesUpdateService, ApprenticeshipProgrammesUpdateService>();
            services.AddTransient<IBankHolidayUpdateService, BankHolidayUpdateService>();
            services.AddTransient<IBankHolidayProvider, BankHolidayProvider>();

            // External client dependencies
            services.AddApprenticeshipsApi(configuration);
        }

        private static void RegisterRepositories(IServiceCollection services, IConfiguration configuration)
        {
            var mongoConnectionString = configuration.GetConnectionString("MongoDb");

            services.Configure<MongoDbConnectionDetails>(options =>
            {
                options.ConnectionString = mongoConnectionString;
            });

            MongoDbConventions.RegisterMongoConventions();

            services.AddTransient<MongoDbCollectionChecker>();

            //Repositories
            services.AddTransient<IVacancyRepository, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewRepository, MongoDbVacancyReviewRepository>();
            services.AddTransient<IUserRepository, MongoDbUserRepository>();
            services.AddTransient<IApplicationReviewRepository, MongoDbApplicationReviewRepository>();
            services.AddTransient<IEmployerProfileRepository, MongoDbEmployerProfileRepository>();
            services.AddTransient<IReportRepository, MongoDbReportRepository>();
            services.AddTransient<IUserNotificationPreferencesRepository, MongoDbUserNotificationPreferencesRepository>();

            //Queries
            services.AddTransient<IVacancyQuery, MongoDbVacancyRepository>();
            services.AddTransient<IVacancyReviewQuery, MongoDbVacancyReviewRepository>();
            services.AddTransient<IApplicationReviewQuery, MongoDbApplicationReviewRepository>();

            //services.AddTransient<IQueryStore, MongoQueryStore>();
            services.AddTransient<IQueryStoreReader, QueryStoreClient>();
            services.AddTransient<IQueryStoreWriter, QueryStoreClient>();

            services.AddTransient<IReferenceDataReader, MongoDbReferenceDataRepository>();
            services.AddTransient<IReferenceDataWriter, MongoDbReferenceDataRepository>();
        }

        private static void RegisterStorageProviderDeps(IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("QueueStorage");

            services.Configure<StorageQueueConnectionDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            services.AddSingleton(kernal => kernal.GetService<IOptions<StorageQueueConnectionDetails>>().Value);

            services.AddTransient<IEventStore, StorageQueueEventStore>();
        }

        private static void RegisterQueues(IServiceCollection services)
        {
            services.AddTransient<IQueueService, StorageQueueService>();
        }

        private static void RegisterTableStorageProviderDeps(IServiceCollection services, IConfiguration configuration)
        {
            var storageConnectionString = configuration.GetConnectionString("TableStorage");
            var useTableStorageQueryStore = configuration.GetValue<bool>("UseTableStorageQueryStore");
            services.AddTransient<QueryStoreTableChecker>();
            services.Configure<TableStorageConnectionsDetails>(options =>
            {
                options.ConnectionString = storageConnectionString;
            });

            if (useTableStorageQueryStore)
                services.AddTransient<IQueryStore, TableStorageQueryStore>();
            else
                services.AddTransient<IQueryStore, MongoQueryStore>();            
        }

        private static void AddValidation(IServiceCollection services)
        {
            services.AddSingleton<AbstractValidator<Vacancy>, FluentVacancyValidator>();
            services.AddSingleton(typeof(IEntityValidator<,>), typeof(EntityValidator<,>));

            services.AddSingleton<AbstractValidator<ApplicationReview>, ApplicationReviewValidator>();
            services.AddSingleton<AbstractValidator<VacancyReview>, VacancyReviewValidator>();

            services.AddSingleton<AbstractValidator<UserNotificationPreferences>, UserNotificationPreferencesValidator>();
        }

        private static void AddRules(IServiceCollection services)
        {
            services.AddTransient<RuleSet<Vacancy>, VacancyRuleSet>();
        }

        private static void RegisterClients(IServiceCollection services)
        {
            services
                .AddTransient<IRecruitVacancyClient, VacancyClient>()
                .AddTransient<IEmployerVacancyClient, VacancyClient>()
                .AddTransient<IProviderVacancyClient, VacancyClient>()
                .AddTransient<IQaVacancyClient, QaVacancyClient>()
                .AddTransient<IJobsVacancyClient, VacancyClient>();
        }

        private static void RegisterProviderApiClientDep(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IProviderApiClient>(_ => new ProviderApiClient(configuration.GetValue<string>("ProviderApiUrl")));
        }

        private static void RegisterMediatR(IServiceCollection services)
        {
            services.AddMediatR(typeof(CreateEmployerOwnedVacancyCommandHandler).Assembly);
            services
                .AddTransient<IMessaging, MediatrMessaging>()
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestPerformanceBehaviour<,>));
        }
    }
}
