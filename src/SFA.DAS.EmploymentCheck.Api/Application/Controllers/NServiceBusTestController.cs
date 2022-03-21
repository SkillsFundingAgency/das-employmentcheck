using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.Abstractions;
using SFA.DAS.EmploymentCheck.Api.Application.Models;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;
using SFA.DAS.EmploymentCheck.Commands.PublishEmploymentCheckResult;

namespace SFA.DAS.EmploymentCheck.Api.Application.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class NServiceBusTestController : ApiCommandControllerBase
    {

        public NServiceBusTestController(ICommandDispatcher commandDispatcher) : base(commandDispatcher)
        {
        }

        [HttpGet]
        [Route("Test")]
        public async Task<IActionResult> Execute()
        {
            await SendCommandAsync(new EmploymentCheckCompletedEvent(new Data.Models.EmploymentCheck
            {
                AccountId = 123456,
                ApprenticeshipId = 223456,
                Uln = 312323323,
                CheckType = "CHECK_TYPE",
                CorrelationId = Guid.NewGuid(),
                CreatedOn = DateTime.Now,
                Employed = true,
                Id = 123,
                RequestCompletionStatus = 1,
                LastUpdatedOn = DateTime.Now,
                MinDate = DateTime.Now.AddMonths(-6),
                MaxDate = DateTime.Now
            }));

            return Ok();
        }
    }
}