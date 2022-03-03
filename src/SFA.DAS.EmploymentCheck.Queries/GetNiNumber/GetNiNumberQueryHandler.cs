using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Clients.Learner;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumber
{
    public class GetNiNumberQueryHandler : IQueryHandler<GetNiNumberQueryRequest, GetNiNumberQueryResult>
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

            var learnerDbNiNumber = await _learnerClient.GetDbNiNumber(getNiNumbersQueryRequest.Check);
            if(learnerDbNiNumber.Uln != 0)
            {
                return new GetNiNumberQueryResult(learnerDbNiNumber);
            }

            var learnerNiNumber = await _learnerClient.GetNiNumber(getNiNumbersQueryRequest.Check);
            if (learnerNiNumber == null)
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
            }

            return new GetNiNumberQueryResult(learnerNiNumber);
        }
    }
}
