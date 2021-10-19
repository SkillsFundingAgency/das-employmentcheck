using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetLearnersRequiringEmploymentCheck
{
    public class GetLearnersRequiringEmploymentCheckHandler : IRequestHandler<GetLearnersRequiringEmploymentCheckRequest, GetLearnersRequiringEmploymentCheckResult>
    {
        private IEmploymentChecksRepository _repository;
        private ILogger<GetLearnersRequiringEmploymentCheckHandler> _logger;

        public GetLearnersRequiringEmploymentCheckHandler(
            ILogger<GetLearnersRequiringEmploymentCheckHandler> logger)
        {
            _logger = logger;
        }

        public async Task<GetLearnersRequiringEmploymentCheckResult> Handle(
            GetLearnersRequiringEmploymentCheckRequest request,
            CancellationToken cancellationToken)
        {
            var thisMethodName = "\n\n GetLearnersRequiringEmploymentCheckHandler.Handle();\n\n";

            List<LearnersRequiringEmploymentCheckDto> learnersRequiringEmploymentCheckDtos = null;

            try
            {

               // TODO: Call the API

                if (learnersRequiringEmploymentCheckDtos == null)
                {
                    learnersRequiringEmploymentCheckDtos = new List<LearnersRequiringEmploymentCheckDto>(); // return empty list rather than null
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"{thisMethodName} Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return await Task.FromResult(new GetLearnersRequiringEmploymentCheckResult(learnersRequiringEmploymentCheckDtos));
        }
    }
}
