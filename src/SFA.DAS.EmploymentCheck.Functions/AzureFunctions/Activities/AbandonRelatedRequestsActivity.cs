using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.CreateEmploymentCheckCacheRequest;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class AbandonRelatedRequestsActivity
    {
        private readonly ICommandDispatcher _dispatcher;

        public AbandonRelatedRequestsActivity(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(AbandonRelatedRequestsActivity))]
        public async Task Create([ActivityTrigger] EmploymentCheckCacheRequest[] employmentCheckCacheRequests)
        {
            await _dispatcher.Send(new CreateEmploymentCheckCacheRequestCommand(employmentCheckCacheRequests));
        }
    }
}