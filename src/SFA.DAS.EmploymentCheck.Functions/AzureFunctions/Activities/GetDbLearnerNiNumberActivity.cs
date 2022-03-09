using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetDbLearnerNiNumberActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public GetDbLearnerNiNumberActivity(IQueryDispatcher dispatcher)
        {
           _dispatcher = dispatcher;
        }

        [FunctionName(nameof(GetDbLearnerNiNumberActivity))]
        public async Task<LearnerNiNumber> Get(
            [ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            var result = await _dispatcher.Send<GetDbNiNumberQueryRequest, GetDbNiNumberQueryResult>(new GetDbNiNumberQueryRequest(employmentCheck));

            return result.LearnerNiNumber;
        }
    }
}

