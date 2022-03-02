using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.Learner
{
    public class LearnerClient : ILearnerClient
    {
        private readonly ILearnerService _learnerService;

        public LearnerClient(ILearnerService learnerService)
        {
            _learnerService = learnerService;
        }

        public async Task<LearnerNiNumber> GetDbNiNumber(Data.Models.EmploymentCheck check)
        {
            var learnerNiNumbers = await _learnerService.GetDbNiNumber(check);

            return learnerNiNumbers;
        }


        public async Task<LearnerNiNumber> GetNiNumber(Data.Models.EmploymentCheck check)
        {
            var learnerNiNumbers = await _learnerService.GetNiNumber(check);

            return learnerNiNumbers;
        }
    }
}