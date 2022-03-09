using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;

namespace SFA.DAS.EmploymentCheck.Queries.GetNiNumber
{
    public class GetNiNumberQueryHandler : IQueryHandler<GetNiNumberQueryRequest, GetNiNumberQueryResult>
    {
        private readonly ILearnerService _service;
        private readonly ILogger<GetNiNumberQueryHandler> _logger;

        public GetNiNumberQueryHandler(
            ILearnerService service,
            ILogger<GetNiNumberQueryHandler> logger)
        {
            _service = service;
            _logger = logger;
        }

        public async Task<GetNiNumberQueryResult> Handle(
            GetNiNumberQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var thisMethodName = $"{nameof(GetNiNumberQueryHandler)}.Handle";

            Guard.Against.Null(request, nameof(request));
            Guard.Against.Null(request.Check, nameof(request.Check));

            var learnerDbNiNumber = await _service.GetDbNiNumber(request.Check);
            if (learnerDbNiNumber != null)
            {
                return new GetNiNumberQueryResult(learnerDbNiNumber);
            }

            var learnerNiNumber = await _service.GetNiNumber(request.Check);
            if (learnerNiNumber == null)
            {
                _logger.LogInformation($"{thisMethodName} returned null/zero NiNumbers");
            }

            return new GetNiNumberQueryResult(learnerNiNumber);
        }
    }
}
