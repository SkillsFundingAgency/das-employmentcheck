using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessEmploymentCheckRequestsOrchestrator
    {
        private readonly ILogger<ProcessEmploymentCheckRequestsOrchestrator> _logger;

        public ProcessEmploymentCheckRequestsOrchestrator(ILogger<ProcessEmploymentCheckRequestsOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessEmploymentCheckRequestsOrchestrator))]
        public async Task ProcessEmploymentCheckRequestsOrchestratorTask([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            EmploymentCheckCacheRequest[] employmentCheckRequests;

            do
            {
                var stopwatch = Stopwatch.StartNew();

                employmentCheckRequests = await context.CallActivityAsync<EmploymentCheckCacheRequest[]>(nameof(GetEmploymentCheckCacheRequestActivity), null);

                await ProcessEmploymentCheckRequest(context, employmentCheckRequests);

                stopwatch.Stop();

                _logger.LogInformation($"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)}: Finished Processing Batch of EmploymentCheckRequests in {stopwatch.Elapsed.TotalMilliseconds}.");

            } while (employmentCheckRequests != null && employmentCheckRequests.Any());

            _logger.LogInformation($"\n\n{nameof(ProcessEmploymentCheckRequestsOrchestrator)}: {nameof(GetEmploymentCheckCacheRequestActivity)} returned no results. Nothing to process.");
        }

        private static async Task ProcessEmploymentCheckRequest(IDurableOrchestrationContext context, EmploymentCheckCacheRequest[] employmentCheckRequests)
        {
            if (employmentCheckRequests == null || !employmentCheckRequests.Any()) return;

            var getEmploymentStatusTasks = employmentCheckRequests.Select(request => context.CallActivityAsync<EmploymentCheckCacheRequest>(nameof(GetHmrcLearnerEmploymentStatusActivity), request));

            var completedRequests = await Task.WhenAll(getEmploymentStatusTasks);

            await context.CallActivityAsync(nameof(AbandonRelatedRequestsActivity), completedRequests);
        }
    }
}