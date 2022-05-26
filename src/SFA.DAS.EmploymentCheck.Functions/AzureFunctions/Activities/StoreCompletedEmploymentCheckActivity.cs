using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Commands.StoreCompletedEmploymentCheck;
using SFA.DAS.EmploymentCheck.Data.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class StoreCompletedEmploymentCheckActivity
    {
        private readonly ICommandDispatcher _dispatcher;

        public StoreCompletedEmploymentCheckActivity(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(StoreCompletedEmploymentCheckActivity))]
        public async Task Store([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            await _dispatcher.Send(new StoreCompletedEmploymentCheckCommand(employmentCheckData.EmploymentCheck));
        }
    }
}