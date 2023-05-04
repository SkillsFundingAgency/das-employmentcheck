using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;

namespace SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces
{
    public interface IApiOptionsRepository
    {
        ApiRetryOptions GetOptions();
    }
}
