using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;

namespace SFA.DAS.EmploymentCheck.Queries.GetHmrcLearnerEmploymentStatus
{
    public class GetHmrcLearnerEmploymentStatusQueryHandler
        : IQueryHandler<GetHmrcLearnerEmploymentStatusQueryRequest,
            GetHmrcLearnerEmploymentStatusQueryResult>
    {
        private readonly IHmrcService _service;
        private readonly IServiceProvider _serviceProvider;

        public GetHmrcLearnerEmploymentStatusQueryHandler(IHmrcService service, [NotNull] IServiceProvider serviceProvider)
        {
            _service = service;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<GetHmrcLearnerEmploymentStatusQueryResult> Handle(GetHmrcLearnerEmploymentStatusQueryRequest request, CancellationToken cancellationToken)
        {
            using var serviceScope = _serviceProvider.CreateScope();
            
            var employmentCheckCacheRequest = await _service.IsNationalInsuranceNumberRelatedToPayeScheme(request.EmploymentCheckCacheRequest);
            
            return new GetHmrcLearnerEmploymentStatusQueryResult(employmentCheckCacheRequest);
        }
    }
}
