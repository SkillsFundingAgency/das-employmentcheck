using Ardalis.GuardClauses;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Queries.ResetEmploymentChecksMessageSentDate
{
    public class ResetEmploymentChecksMessageSentDateQueryHandler
        : IQueryHandler<ResetEmploymentChecksMessageSentDateQueryRequest,
            ResetEmploymentChecksMessageSentDateQueryResult>
    {
        private readonly IEmploymentCheckService _service;

        public ResetEmploymentChecksMessageSentDateQueryHandler(
            IEmploymentCheckService service)
        {
            _service = service;
        }

        public async Task<ResetEmploymentChecksMessageSentDateQueryResult> Handle(
            ResetEmploymentChecksMessageSentDateQueryRequest request,
            CancellationToken cancellationToken = default
        )
        {
            Guard.Against.Null(request, nameof(request));
            var args = request.EmploymentCheckMessageSentData;
            long updatedRowsCount = 0;

            if (args.Contains("correlationid"))
            {
                var correlationIdString = args.Split("=")[1];
                Guid CorrelationId = Guid.Parse(correlationIdString);
                updatedRowsCount = await _service.ResetEmploymentChecksMessageSentDate(CorrelationId);
            }
            else
            {
                var temp = args.Split("=")[1];
                var messageSentFromDate = Convert.ToDateTime(temp.Split('&')[0]);
                var messageSentToDate = Convert.ToDateTime(args.Split("=")[2]);
                updatedRowsCount = await _service.ResetEmploymentChecksMessageSentDate(messageSentFromDate, messageSentToDate);
            }

            return new ResetEmploymentChecksMessageSentDateQueryResult(updatedRowsCount);
        }
    }
}