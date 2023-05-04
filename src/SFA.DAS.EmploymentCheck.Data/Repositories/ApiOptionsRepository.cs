using Microsoft.Extensions.Options;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    [ExcludeFromCodeCoverage]
    public class ApiOptionsRepository : IApiOptionsRepository
    {
        private readonly ApiRetryOptions _apiRetryOptions;

        public ApiOptionsRepository(IOptions<ApiRetryOptions> apiRetryOptions)
        {
            _apiRetryOptions = apiRetryOptions.Value;
        }

        public ApiRetryOptions GetOptions()
        {
            return _apiRetryOptions;
        }

    }
}
