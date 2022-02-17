using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner
{
    public class LearnerClient
        : ILearnerClient
    {
        private readonly ILearnerService _learnerService;

        public LearnerClient(ILearnerService learnerService)
        {
            _learnerService = learnerService;
        }

        public async Task<IList<LearnerNiNumber>> GetNiNumbers(IList<Models.EmploymentCheck> apprentices)
        {
            var learnerNiNumbers = await _learnerService.GetNiNumbers(apprentices);

            return learnerNiNumbers;
        }
    }
}