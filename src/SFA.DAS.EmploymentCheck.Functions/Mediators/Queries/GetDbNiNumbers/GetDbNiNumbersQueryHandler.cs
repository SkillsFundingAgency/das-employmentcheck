using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Models = SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetDbNiNumbers
{
    public class GetDbNiNumbersQueryHandler
        : IRequestHandler<GetDbNiNumbersQueryRequest,
            GetDbNiNumbersQueryResult>
    {
        private readonly ILearnerService _learnerService;
        private readonly IDataCollectionsResponseRepository _repository;
        private readonly ILogger<GetDbNiNumbersQueryHandler> _logger;

        public GetDbNiNumbersQueryHandler(
            ILearnerService learnerClient,
            IDataCollectionsResponseRepository repository,
            ILogger<GetDbNiNumbersQueryHandler> logger)
        {
            _learnerService = learnerClient;
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetDbNiNumbersQueryResult> Handle(
            GetDbNiNumbersQueryRequest getDbNiNumbersQueryRequest,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{nameof(GetDbNiNumbersQueryHandler)}.Handle";

            Guard.Against.Null(getDbNiNumbersQueryRequest, nameof(getDbNiNumbersQueryRequest));
            Guard.Against.Null(getDbNiNumbersQueryRequest.EmploymentCheckBatch, nameof(getDbNiNumbersQueryRequest.EmploymentCheckBatch));

            var learnerNiNumbers = await GetDbNiNumbers(getDbNiNumbersQueryRequest.EmploymentCheckBatch);

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

        private async Task<IList<LearnerNiNumber>> GetDbNiNumbers(IList<Models.EmploymentCheck> employmentCheckBatch)
        {
            var learnerNiNumbers = new List<LearnerNiNumber>();
            foreach (var employmentCheck in employmentCheckBatch)
            {
                LearnerNiNumber learnerNiNumber = null;
                var response = await _repository.GetByEmploymentCheckId(employmentCheck.Id);
                if (response != null)
                {
                    learnerNiNumber = new LearnerNiNumber { Uln = employmentCheck.Uln, NiNumber = response.NiNumber };
                    learnerNiNumbers.Add(learnerNiNumber);
                }
            }

            return learnerNiNumbers;
        }
    }
}
