﻿using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyRepository
    {
        Task CreateAsync(Vacancy vacancy);
        Task UpdateAsync(Vacancy vacancy);
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task<Vacancy> GetVacancyAsync(long vacancyReference);
        Task<IEnumerable<T>> GetVacanciesByEmployerAccountAsync<T>(string employerAccountId);
        Task<Vacancy> GetSingleVacancyForPostcode(string postcode);
    }
}