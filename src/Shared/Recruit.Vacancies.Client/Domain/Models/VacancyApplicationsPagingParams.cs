using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Models;

public record VacancyApplicationsPagingParams(SortColumn SortColumn, SortOrder SortOrder, int PageNumber, int PageSize);