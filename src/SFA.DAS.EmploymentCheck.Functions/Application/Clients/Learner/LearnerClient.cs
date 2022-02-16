using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner
{
    public class LearnerClient : ILearnerClient
    {
        private readonly ILogger<IEmploymentCheckClient> _logger;
        private readonly ILearnerService _learnerService;

        public LearnerClient(
            ILogger<IEmploymentCheckClient> logger,
            ILearnerService learnerService)
        {
            _logger = logger;
            _learnerService = learnerService;
        }

        public async Task<LearnerNiNumber> GetNiNumber(Models.EmploymentCheck check)
        {
            var learnerNiNumbers = await _learnerService.GetNiNumber(check);

            return learnerNiNumbers;
        }
    }
}