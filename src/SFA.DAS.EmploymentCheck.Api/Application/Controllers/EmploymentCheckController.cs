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
        public async Task<IActionResult> RegisterCheck(RegisterCheckRequest registerCheckDto)
        {
            var commandResponse = await _mediator.Send(new RegisterCheckCommand
            {
                CorrelationId = registerCheckDto.CorrelationId,
                CheckType = registerCheckDto.CheckType ?? "",
                Uln = registerCheckDto.Uln,
                ApprenticeshipAccountId = registerCheckDto.ApprenticeshipAccountId,
                ApprenticeshipId = registerCheckDto.ApprenticeshipId,
                MinDate = registerCheckDto.MinDate,
                MaxDate = registerCheckDto.MaxDate
            });

            var response = new Responses.RegisterCheckResponse();

            if (commandResponse.VersionId != null)
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