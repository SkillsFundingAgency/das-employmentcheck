using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers
{
    public class GetDbNiNumbersQueryHandler
        : IRequestHandler<GetDbNiNumbersQueryRequest,
            GetDbNiNumbersQueryResult>
    {
        private readonly ILearnerService _learnerService;
        private readonly ILogger<GetDbNiNumbersQueryHandler> _logger;

        public GetDbNiNumbersQueryHandler(
            ILearnerService learnerService,
            ILogger<GetDbNiNumbersQueryHandler> logger)
        {
            _learnerService = learnerService;
            _logger = logger;
        }

        public async Task<GetDbNiNumbersQueryResult> Handle(
            GetDbNiNumbersQueryRequest getDbNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetDbNiNumbersQueryHandler)}.Handle";

            Guard.Against.Null(getDbNiNumbersQueryRequest, nameof(getDbNiNumbersQueryRequest));
            Guard.Against.Null(getDbNiNumbersQueryRequest.EmploymentCheckBatch, nameof(getDbNiNumbersQueryRequest.EmploymentCheckBatch));

            var learnerNiNumbers = await _learnerService.GetDbNiNumbers(getDbNiNumbersQueryRequest.EmploymentCheckBatch);

            if (learnerNiNumbers != null &&
                learnerNiNumbers.Count > 0)
            {
                _logger.LogInformation($"{thisMethodName} returned {learnerNiNumbers.Count} NiNumbers");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
                learnerNiNumbers = new List<LearnerNiNumber>(); // return empty list rather than null
            }

            return new GetDbNiNumbersQueryResult(learnerNiNumbers);
        }
    }
}
