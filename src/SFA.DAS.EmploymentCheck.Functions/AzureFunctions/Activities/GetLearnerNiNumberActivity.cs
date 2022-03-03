using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetNiNumber;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetLearnerNiNumberActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public GetLearnerNiNumberActivity(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(GetLearnerNiNumberActivity))]
        public async Task<LearnerNiNumber> Get([ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            if (employmentCheck == null)
            {
                return new LearnerNiNumber();
            }

            var getLearnerNiNumbersQueryResult = await _dispatcher.Send<GetNiNumberQueryRequest, GetNiNumberQueryResult>(new GetNiNumberQueryRequest(employmentCheck));

            return getLearnerNiNumbersQueryResult.LearnerNiNumber;
        }
    }
}

