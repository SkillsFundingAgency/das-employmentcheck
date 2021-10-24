using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumbersMediatorHandler
        : IRequestHandler<GetApprenticesNiNumberMediatorRequest,
            GetApprenticesNiNumberMediatorResult>
    {
        private ISubmitLearnerDataClient _submitLearnerDataClient;
        private ILogger<GetApprenticesNiNumbersMediatorHandler> _logger;

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
            var thisMethodName = "GetApprenticesNiNumbersHandler.Handle(): ";

            IList<ApprenticeNiNumber> apprenticeNiNumbers = null;

            try
            {
                apprenticeNiNumbers = await _submitLearnerDataClient.GetApprenticesNiNumber(request.Apprentices);

                if (apprenticeNiNumbers == null)
                {
                    apprenticeNiNumbers = new List<ApprenticeNiNumber>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"\n\n{thisMethodName}Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(new GetApprenticesNiNumberMediatorResult(apprenticeNiNumbers));
        }
    }
}
