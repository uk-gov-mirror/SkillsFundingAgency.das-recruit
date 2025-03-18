using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Models;

public record PagedResults<T>(IEnumerable<T> Items, int TotalCount, int CurrentPage, int PageSize);