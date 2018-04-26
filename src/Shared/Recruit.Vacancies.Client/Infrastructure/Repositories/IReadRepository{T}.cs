using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Repositories
{
    public interface IReadRepository<T, TKeyType> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(TKeyType id);
    }
}