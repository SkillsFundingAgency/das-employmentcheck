using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryHandler
        : IRequestHandler<GetPayeSchemesQueryRequest,
            GetPayeSchemesQueryResult>
    {
        private readonly ILogger<GetPayeSchemesQueryHandler> _logger;
        private readonly IEmployerAccountClient _employerAccountClient;

        public GetPayeSchemesQueryHandler(
            ILogger<GetPayeSchemesQueryHandler> logger,
            IEmployerAccountClient employerAccountClient)
        {
            _logger = logger;
            _employerAccountClient = employerAccountClient;
        }

        public async Task<GetPayeSchemesQueryResult> Handle(
            GetPayeSchemesQueryRequest getPayeSchemesRequest,
            CancellationToken cancellationToken)
        {
            const string thisMethodName = "GetPayeSchemesQueryHandler.Handle";

            Guard.Against.Null(getPayeSchemesRequest, nameof(getPayeSchemesRequest));
            Guard.Against.Null(getPayeSchemesRequest.EmploymentCheckBatch, nameof(getPayeSchemesRequest.EmploymentCheckBatch));

            var employersPayeSchemes =
                await _employerAccountClient.GetEmployersPayeSchemes(getPayeSchemesRequest.EmploymentCheckBatch) ?? new EmployerPayeSchemes();

            if (employersPayeSchemes != null && employersPayeSchemes.EmployerAccountId != 0)
            {
                _logger.LogInformation($"{thisMethodName} returned {employersPayeSchemes.PayeSchemes.Count} PAYE scheme(s)");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero PAYE schemes");
                employersPayeSchemes = new EmployerPayeSchemes(); // return blank paye rather than null
            }

            return new GetPayeSchemesQueryResult(employersPayeSchemes);
        }
    }
}