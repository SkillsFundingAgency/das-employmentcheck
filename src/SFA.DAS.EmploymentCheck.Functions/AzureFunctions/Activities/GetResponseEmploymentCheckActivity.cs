using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetResponseEmploymentCheck;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetResponseEmploymentCheckActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public GetResponseEmploymentCheckActivity(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(GetResponseEmploymentCheckActivity))]
        public async Task<Data.Models.EmploymentCheck> Get(
            [ActivityTrigger] object input)
        {
            var result = await _dispatcher.Send<GetResponseEmploymentCheckQueryRequest, GetResponseEmploymentCheckQueryResult>(new GetResponseEmploymentCheckQueryRequest());

            return result.EmploymentCheck;
        }
    }
}