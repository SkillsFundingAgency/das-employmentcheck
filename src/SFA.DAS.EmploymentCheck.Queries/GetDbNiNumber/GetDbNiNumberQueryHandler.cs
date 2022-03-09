using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetDbNiNumber
{
    public class GetDbNiNumberQueryHandler
        : IQueryHandler<GetDbNiNumberQueryRequest,
            GetDbNiNumberQueryResult>
    {
        private readonly ILearnerService _service;
        private readonly ILogger<GetDbNiNumberQueryHandler> _logger;

        public GetDbNiNumberQueryHandler(
            ILearnerService service,
            ILogger<GetDbNiNumberQueryHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<GetDbNiNumberQueryResult> Handle(
            GetDbNiNumberQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var thisMethodName = $"{nameof(GetDbNiNumberQueryHandler)}.Handle";

            Guard.Against.Null(request, nameof(request));
            Guard.Against.Null(request.EmploymentCheck, nameof(request.EmploymentCheck));

            var learnerNiNumber = await _service.GetDbNiNumber(request.EmploymentCheck);

            if (learnerNiNumber != null && learnerNiNumber.NiNumber != null)
            {
                _logger.LogInformation($"{thisMethodName} returned an NiNumber for {learnerNiNumber.Uln}");
            }
            else
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
            }

            return new GetDbNiNumberQueryResult(learnerNiNumber);
        }
    }
}
