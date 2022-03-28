using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class OutputEmploymentCheckResultsActivity
    {
        private readonly ICommandDispatcher _dispatcher;

        public OutputEmploymentCheckResultsActivity(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(OutputEmploymentCheckResultsActivity))]
        public async Task Send([ActivityTrigger] Data.Models.EmploymentCheck employmentCheck)
        {
            await _dispatcher.Send(new PublishEmploymentCheckResultCommand(employmentCheck));
        }
    }
}