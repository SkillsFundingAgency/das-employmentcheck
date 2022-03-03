using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetEmploymentCheck;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetEmploymentCheckActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public GetEmploymentCheckActivity(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(GetEmploymentCheckActivity))]
        public async Task<Data.Models.EmploymentCheck> Get(
            [ActivityTrigger] object input)
        {
            var result = await _dispatcher.Send<GetEmploymentCheckQueryRequest, GetEmploymentCheckQueryResult>(new GetEmploymentCheckQueryRequest());

            return result.EmploymentCheck ?? new Data.Models.EmploymentCheck();
        }
    }
}