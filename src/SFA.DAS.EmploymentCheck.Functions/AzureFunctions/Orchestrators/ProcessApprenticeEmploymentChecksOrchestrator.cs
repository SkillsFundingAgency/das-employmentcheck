using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Orchestrators
{
    public class ProcessApprenticeEmploymentChecksOrchestrator
    {
        private const string ThisClassName = "\n\nProcessApprenticeEmploymentChecksSubOrchestrator";

        private ILogger<ProcessApprenticeEmploymentChecksOrchestrator> _logger;

        public ProcessApprenticeEmploymentChecksOrchestrator(
            ILogger<ProcessApprenticeEmploymentChecksOrchestrator> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessApprenticeEmploymentChecksOrchestrator))]
        public async Task ProcessApprenticeEmploymentChecksSubOrchestratorTask(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var thisMethodName = $"{ThisClassName}.ProcessApprenticeEmploymentChecksSubOrchestratorTask()";

            try
            {
                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Started.");

                // Get the next message off the message queue
                var apprenticeEmploymentCheckMessage = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(nameof(DequeueApprenticeEmploymentCheckMessageActivity), null);

                // Do the employment status check on this batch of messages
                var updatedApprenticeEmploymentCheckMessage = await context.CallActivityAsync<ApprenticeEmploymentCheckMessageModel>(nameof(CheckApprenticeEmploymentStatusActivity), apprenticeEmploymentCheckMessage);

                // Save the employment status back to the database
                await context.CallActivityAsync(nameof(SaveApprenticeEmploymentCheckResultActivity), updatedApprenticeEmploymentCheckMessage);

                if (!context.IsReplaying)
                    _logger.LogInformation($"{thisMethodName}: Completed.");

                // execute the orchestrator again with a new context to process the next message
                // Note: The orchestrator may have been unloaded from memory whilst the activity
                // functions were running so this could be a new instance of the orchestrator which
                // will run though the stored 'event sourcing' state from table storage
                context.ContinueAsNew(null);
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName} Exception caught: {ex.Message}. {ex.StackTrace}");
            }
        }
    }
}
