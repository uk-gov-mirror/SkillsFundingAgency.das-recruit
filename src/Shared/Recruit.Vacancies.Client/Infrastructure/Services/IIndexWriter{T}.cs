using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services
{
    public interface IIndexWriter<T>
    {
        Task<bool> IndexAsync(string indexName, T item);
    }
}