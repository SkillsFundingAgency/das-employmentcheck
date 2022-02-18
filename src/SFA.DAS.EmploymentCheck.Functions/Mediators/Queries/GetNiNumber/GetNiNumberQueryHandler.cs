using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumber
{
    public class GetNiNumberQueryHandler : IRequestHandler<GetNiNumberQueryRequest, GetNiNumberQueryResult>
    {
        private readonly ILearnerClient _learnerClient;
        private readonly ILogger<GetNiNumberQueryHandler> _logger;

        public GetNiNumberQueryHandler(
            ILearnerClient learnerClient,
            ILogger<GetNiNumberQueryHandler> logger)
        {
            _learnerClient = learnerClient;
            _logger = logger;
        }

        public async Task<GetNiNumberQueryResult> Handle(
            GetNiNumberQueryRequest getNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetNiNumberQueryHandler)}.Handle";

            Guard.Against.Null(getNiNumbersQueryRequest, nameof(getNiNumbersQueryRequest));
            Guard.Against.Null(getNiNumbersQueryRequest.Check, nameof(getNiNumbersQueryRequest.Check));

            var learnerNiNumber = await _learnerClient.GetNiNumber(getNiNumbersQueryRequest.Check);

            if (learnerNiNumber == null)
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
            }

            return new GetNiNumberQueryResult(learnerNiNumber);
        }
    }
}
