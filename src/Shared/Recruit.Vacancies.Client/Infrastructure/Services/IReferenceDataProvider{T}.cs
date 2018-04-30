using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IReferenceDataProvider<T, TKeyType> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(TKeyType id);
    }
}