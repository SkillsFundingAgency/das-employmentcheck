using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
{
    public class GetPayeSchemeQueryHandler
        : IRequestHandler<GetPayeSchemesQueryRequest,
            GetPayeSchemesQueryResult>
    {
        private readonly ILogger<GetPayeSchemeQueryHandler> _logger;
        private readonly IEmployerAccountClient _employerAccountClient;

        public GetPayeSchemeQueryHandler(
            ILogger<GetPayeSchemeQueryHandler> logger,
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
            Guard.Against.Null(getPayeSchemesRequest.EmploymentCheck, nameof(getPayeSchemesRequest.EmploymentCheck));

            var employersPayeSchemes =
                await _employerAccountClient.GetEmployersPayeSchemes(getPayeSchemesRequest.EmploymentCheck) ?? new EmployerPayeSchemes();

            if (employersPayeSchemes.EmployerAccountId != 0)
            {
                _logger.LogInformation($"{thisMethodName} returned {employersPayeSchemes.PayeSchemes.Count} PAYE scheme(s)");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero PAYE schemes");
                employersPayeSchemes = new EmployerPayeSchemes();
            }

            return new GetPayeSchemesQueryResult(employersPayeSchemes);
        }
    }
}