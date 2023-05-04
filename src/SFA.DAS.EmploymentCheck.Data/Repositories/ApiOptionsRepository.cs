using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    [ExcludeFromCodeCoverage]
    public class ApiOptionsRepository : IApiOptionsRepository
    {
        private ApiRetryOptions _apiRetryOptions;

        public ApiOptionsRepository(IOptions<ApiRetryOptions> apiRetryOptions)
        {
            _apiRetryOptions = apiRetryOptions.Value;
        }

        public async Task<ApiRetryOptions> GetOptions()
        {
            return _apiRetryOptions;
        }

    }
}
