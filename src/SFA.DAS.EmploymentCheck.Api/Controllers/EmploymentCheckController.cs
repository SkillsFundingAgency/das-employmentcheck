using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.Api.Commands;
using SFA.DAS.EmploymentCheck.Api.Responses;

namespace SFA.DAS.EmploymentCheck.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]/")]
    public class EmploymentCheckController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmploymentCheckController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Route("RegisterCheck")]
        public async Task<IActionResult> RegisterCheck(
            [FromQuery] Guid correlationId,
            [FromQuery] string checkType,
            [FromQuery] long uln,
            [FromQuery] int apprenticeshipAccountId,
            [FromQuery] long? apprenticeshipId,
            [FromQuery] DateTime minDate,
            [FromQuery] DateTime maxDate)
        {
            var commandResponse = await _mediator.Send(new PostRegisterCheckCommand
            {
                CorrelationId = correlationId,
                CheckType = checkType,
                Uln = uln,
                ApprenticeshipAccountId = apprenticeshipAccountId,
                ApprenticeshipId = apprenticeshipId,
                MinDate = minDate,
                MaxDate = maxDate
            });

            var response = new RegisterCheckResponse();

            if (commandResponse.VersionId != "0")
            {
                response.VersionId = commandResponse.VersionId;

                return Ok(response);
            }

            response.ErrorMessage = commandResponse.ErrorMessage;
            response.ErrorType = commandResponse.ErrorType;
            response.VersionId = commandResponse.VersionId;

            return BadRequest(response);
        }
    }
}