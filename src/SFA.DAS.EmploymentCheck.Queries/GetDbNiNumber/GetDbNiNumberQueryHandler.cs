using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryHandler
        : IRequestHandler<GetDbNiNumberQueryRequest,
            GetDbNiNumberQueryResult>
    {
        private readonly ILearnerService _learnerService;
        private readonly ILogger<GetDbNiNumberQueryHandler> _logger;

        public GetDbNiNumberQueryHandler(
            ILearnerService learnerService,
            ILogger<GetDbNiNumberQueryHandler> logger)
        {
            _learnerService = learnerService;
            _logger = logger;
        }

        public async Task<GetDbNiNumberQueryResult> Handle(
            GetDbNiNumberQueryRequest getDbNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetDbNiNumberQueryHandler)}.Handle";

            Guard.Against.Null(getDbNiNumbersQueryRequest, nameof(getDbNiNumbersQueryRequest));
            Guard.Against.Null(getDbNiNumbersQueryRequest.EmploymentCheck, nameof(getDbNiNumbersQueryRequest.EmploymentCheck));

            var learnerNiNumber = await _learnerService.GetDbNiNumber(getDbNiNumbersQueryRequest.EmploymentCheck);

            if (learnerNiNumber != null && learnerNiNumber.NiNumber != null)
            {
                _logger.LogInformation($"{thisMethodName} returned an NiNumber for {learnerNiNumber.Uln}");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
                learnerNiNumber = new LearnerNiNumber();
            }

            return new GetDbNiNumberQueryResult(learnerNiNumber);
        }
    }
}
