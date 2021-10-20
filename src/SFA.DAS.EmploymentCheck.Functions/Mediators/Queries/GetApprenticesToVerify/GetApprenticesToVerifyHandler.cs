using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesToVerify
{
    public class GetApprenticesToVerifyHandler : IRequestHandler<GetApprenticesToVerifyRequest, GetApprenticesToVerifyResult>
    {
        private IEmploymentChecksRepository _repository;
        private ILogger<GetApprenticesToVerifyHandler> _logger;

        public GetApprenticesToVerifyHandler(
            IEmploymentChecksRepository repository,
            ILogger<GetApprenticesToVerifyHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<GetApprenticesToVerifyResult> Handle(GetApprenticesToVerifyRequest request, CancellationToken cancellationToken)
        {
            var thisMethodName = "GetApprenticesToVerifyHandler.Handle()";

            List<ApprenticeToVerifyDto> apprenticesToCheck = null;

            try
            {
                // Call the data repository to get the apprentices to check
                apprenticesToCheck = await _repository.GetApprenticesToCheck();

                if(apprenticesToCheck == null)
                {
                    //_logger.LogInformation($"{messagePrefix} [_repository.GetApprenticesToCheck()] returned null/zero apprentices.");
                    apprenticesToCheck = new List<ApprenticeToVerifyDto>(); // return empty list rather than null
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{thisMethodName}\n\n Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return new GetApprenticesToVerifyResult(apprenticesToCheck);
        }
    }
}
