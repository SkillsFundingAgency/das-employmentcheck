using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Commands;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class OutputEmploymentCheckResultsActivityStub
    {
        private readonly ICommandDispatcher _dispatcher;

        public OutputEmploymentCheckResultsActivityStub(ICommandDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(OutputEmploymentCheckResultsActivityStub))]
        public async Task Create([ActivityTrigger] object whatever)
        {
            await _dispatcher.Send(new PublishEmploymentCheckResultCommand(new Data.Models.EmploymentCheck
            {
                AccountId = 123456,
                ApprenticeshipId = 223456,
                Uln = 312323323,
                CheckType = "CHECK_TYPE",
                CorrelationId = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
                Employed = true,
                Id = 123,
                RequestCompletionStatus = 1,
                LastUpdatedOn = DateTime.Now,
                MinDate = DateTime.Now.AddMonths(-6),
                MaxDate = DateTime.Now
            }));
        }
    }
}