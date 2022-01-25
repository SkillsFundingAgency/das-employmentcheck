using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Learner;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumbers
{
    public class GetNiNumbersQueryHandler
        : IRequestHandler<GetNiNumbersQueryRequest,
            GetNiNumbersQueryResult>
    {
        #region Private members
        private readonly ILearnerClient _learnerClient;
        private readonly ILogger<GetNiNumbersQueryHandler> _logger;
        #endregion Private members

        #region Constructors
        public GetNiNumbersQueryHandler(
            ILearnerClient learnerClient,
            ILogger<GetNiNumbersQueryHandler> logger)
        {
            _learnerClient = learnerClient;
            _logger = logger;
        }
        #endregion Constructors

        #region Handle

        public async Task<GetNiNumbersQueryResult> Handle(
            GetNiNumbersQueryRequest getNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetNiNumbersQueryHandler)}.Handle";

            Guard.Against.Null(getNiNumbersQueryRequest, nameof(getNiNumbersQueryRequest));
            Guard.Against.Null(getNiNumbersQueryRequest.EmploymentCheckBatch, nameof(getNiNumbersQueryRequest.EmploymentCheckBatch));

            var learnerNiNumbers = await _learnerClient.GetNiNumbers(getNiNumbersQueryRequest.EmploymentCheckBatch);

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

            return new GetNiNumbersQueryResult(learnerNiNumbers);
        }

        #endregion Handle
    }
}
