using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumbersMediatorHandler
        : IRequestHandler<GetApprenticesNiNumberMediatorRequest,
            GetApprenticesNiNumberMediatorResult>
    {
        private const string ThisClassName = "\n\nDequeueApprenticeEmploymentCheckMessagesQueryHandler";

        private readonly ISubmitLearnerDataClient _submitLearnerDataClient;
        private readonly ILogger<GetApprenticesNiNumbersMediatorHandler> _logger;

        public GetApprenticesNiNumbersMediatorHandler(
            ISubmitLearnerDataClient submitLearnerDataClient,
            ILogger<GetApprenticesNiNumbersMediatorHandler> logger)
        {
            _submitLearnerDataClient = submitLearnerDataClient;
            _logger = logger;
        }

        public async Task<GetApprenticesNiNumberMediatorResult> Handle(
            GetApprenticesNiNumberMediatorRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = $"{ThisClassName}.Handle(): ";

            IList<ApprenticeNiNumber> apprenticeNiNumbers = null;
            try
            {
                apprenticeNiNumbers = await _submitLearnerDataClient.GetApprenticesNiNumber(request.ApprenticeEmploymentCheck) ??
                                      new List<ApprenticeNiNumber>();
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetApprenticesNiNumberMediatorResult(apprenticeNiNumbers);
        }
    }
}
