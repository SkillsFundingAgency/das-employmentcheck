using System.Collections.Generic;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class GetHmrcLearnerEmploymentStatusActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public GetHmrcLearnerEmploymentStatusActivity(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(GetHmrcLearnerEmploymentStatusActivity))]
        public async Task<EmploymentCheckCacheRequest> GetHmrcEmploymentStatusTask([ActivityTrigger] EmploymentCheckCacheRequest request)
        {
            var result = await _dispatcher.Send<GetHmrcLearnerEmploymentStatusQueryRequest, GetHmrcLearnerEmploymentStatusQueryResult>(new GetHmrcLearnerEmploymentStatusQueryRequest(request));

            return result.EmploymentCheckCacheRequest;
        }
    }
}