using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.Api.Application.Models;
using SFA.DAS.EmploymentCheck.Api.Mediators.Commands.RegisterCheckCommand;

namespace SFA.DAS.EmploymentCheck.Api.Application.Controllers
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
        public async Task<IActionResult> RegisterCheck(RegisterCheckRequest registerCheckRequest)
        {
            var commandResponse = await _mediator.Send(new RegisterCheckCommand
            {
                CorrelationId = registerCheckRequest.CorrelationId,
                CheckType = registerCheckRequest.CheckType ?? "",
                Uln = registerCheckRequest.Uln,
                ApprenticeshipAccountId = registerCheckRequest.ApprenticeshipAccountId,
                ApprenticeshipId = registerCheckRequest.ApprenticeshipId,
                MinDate = registerCheckRequest.MinDate,
                MaxDate = registerCheckRequest.MaxDate
            });

            var response = new Responses.RegisterCheckResponse();

            if (commandResponse.VersionId != 0)
            {
                response.VersionId = commandResponse.VersionId;

                return Ok(response);
            }

            response.ErrorMessage = commandResponse.ErrorMessage;
            response.ErrorType = commandResponse.ErrorType;

            return BadRequest(response);
        }
    }
}