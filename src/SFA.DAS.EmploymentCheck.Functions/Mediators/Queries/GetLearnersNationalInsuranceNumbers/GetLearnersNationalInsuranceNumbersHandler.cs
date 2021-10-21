using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersNationalInsuranceNumbers
{
    public class GetLearnersNationalInsuranceNumbersHandler : IRequestHandler<GetLearnersNationalInsuranceNumbersRequest, GetLearnersNationalInsuranceNumbersResult>
    {
        private IEmploymentChecksRepository _repository;
        private ILogger<GetLearnersNationalInsuranceNumbersHandler> _logger;

        public GetLearnersNationalInsuranceNumbersHandler(
            IEmploymentChecksRepository repository,
            ILogger<GetLearnersNationalInsuranceNumbersHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetLearnersNationalInsuranceNumbersResult> Handle(
            GetLearnersNationalInsuranceNumbersRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "\n\n GetLearnersNationalInsuranceNumbersHandler.Handle();\n\n";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            List<LearnerNationalnsuranceNumberDto> learnerNationalnsuranceNumberDto = null;

            try
            {
                bool checkPassed = false;
                int i = 0;

                //checkPassed = await _hmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payeScheme, request, request.Apprentice.StartDate, request.Apprentice.EndDate);

                if (learnerNationalnsuranceNumberDto == null)
                {
                    learnerNationalnsuranceNumberDto = new List<LearnerNationalnsuranceNumberDto>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{messagePrefix} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(new GetLearnersNationalInsuranceNumbersResult(learnerNationalnsuranceNumberDto));
        }
    }
}
