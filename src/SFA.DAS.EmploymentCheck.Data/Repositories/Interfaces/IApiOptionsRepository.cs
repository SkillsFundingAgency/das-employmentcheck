using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IApiOptionsRepository
    {
        Task<ApiRetryOptions> GetOptions();
    }
}
