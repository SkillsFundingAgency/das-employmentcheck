using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CreateEmploymentCheckCacheRequests;

namespace SFA.DAS.EmploymentCheck.Functions.AzureFunctions.Activities
{
    public class CreateEmploymentCheckCacheRequestsActivity
    {
        #region Privatet members
        private const string ThisClassName = "\n\nCreateEmploymentCheckCacheRequestsActivity";
        private readonly IMediator _mediator;
        private readonly ILogger<CreateEmploymentCheckCacheRequestsActivity> _logger;
        #endregion Privatet members

        #region Constructors
        public CreateEmploymentCheckCacheRequestsActivity(
            IMediator mediator,
            ILogger<CreateEmploymentCheckCacheRequestsActivity> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }
        #endregion Constructors

        #region Get
        [FunctionName(nameof(CreateEmploymentCheckCacheRequestsActivity))]
        public async Task Create([ActivityTrigger] EmploymentCheckData employmentCheckData)
        {
            var thisMethodName = $"{ThisClassName}.Create()";

            try
            {
                // Send MediatR request to create the employment check cache requests
                await _mediator.Send(new CreateEmploymentCheckCacheRequestCommand(employmentCheckData));
            }
            catch (Exception ex)
            {
                _logger.LogError($"\n\n{thisMethodName}: Exception caught - {ex.Message}. {ex.StackTrace}");
            }

            return;
        }
        #endregion Get
    }
}