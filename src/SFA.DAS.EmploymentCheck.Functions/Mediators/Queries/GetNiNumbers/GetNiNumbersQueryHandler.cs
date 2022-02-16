using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryHandler : IRequestHandler<GetNiNumbersQueryRequest, GetNiNumbersQueryResult>
    {
        private readonly ILearnerClient _learnerClient;
        private readonly ILogger<GetNiNumbersQueryHandler> _logger;

        public GetNiNumbersQueryHandler(
            ILearnerClient learnerClient,
            ILogger<GetNiNumbersQueryHandler> logger)
        {
            _learnerClient = learnerClient;
            _logger = logger;
        }

        public async Task<GetNiNumbersQueryResult> Handle(
            GetNiNumbersQueryRequest getNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetNiNumbersQueryHandler)}.Handle";

            Guard.Against.Null(getNiNumbersQueryRequest, nameof(getNiNumbersQueryRequest));
            Guard.Against.Null(getNiNumbersQueryRequest.Check, nameof(getNiNumbersQueryRequest.Check));

            var learnerNiNumber = await _learnerClient.GetNiNumber(getNiNumbersQueryRequest.Check);

            if (learnerNiNumber == null)
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
            }

            return new GetNiNumbersQueryResult(learnerNiNumber);
        }
    }
}
