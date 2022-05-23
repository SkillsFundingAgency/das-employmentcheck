using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using SFA.DAS.EmploymentCheck.Queries;
using SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class ResetEmploymentChecksMessageSentDateActivity
    {
        private readonly IQueryDispatcher _dispatcher;

        public ResetEmploymentChecksMessageSentDateActivity(IQueryDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [FunctionName(nameof(ResetEmploymentChecksMessageSentDateActivity))]
        public async Task<long> Reset([ActivityTrigger] string employmentCheckMessageSentData)
        {
            var result = await _dispatcher.Send<ResetEmploymentChecksMessageSentDateQueryRequest, ResetEmploymentChecksMessageSentDateQueryResult>(
                new ResetEmploymentChecksMessageSentDateQueryRequest(employmentCheckMessageSentData));

            return result.UpdatedRowsCount;
        }

    }
}