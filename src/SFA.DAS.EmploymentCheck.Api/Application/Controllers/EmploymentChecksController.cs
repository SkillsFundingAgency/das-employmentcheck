using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.EmploymentCheck.Api.Repositories;
using SFA.DAS.EmploymentCheck.Api.Responses;
using ApplicationModels = SFA.DAS.EmploymentCheck.Api.Application.Models;

namespace SFA.DAS.EmploymentCheck.Api.Application.Controllers;

[ApiController]
[Route("api/employment-checks")]
public class EmploymentChecksController(IEmploymentCheckRepository repository) : ControllerBase
{
    private const int MaxApprenticeshipIds = 50;

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] List<long> apprenticeshipIds)
    {
        if (apprenticeshipIds == null || apprenticeshipIds.Count == 0)
            return BadRequest("apprenticeshipIds is required and must not be empty.");

        if (apprenticeshipIds.Count > MaxApprenticeshipIds)
            return BadRequest($"apprenticeshipIds must not exceed {MaxApprenticeshipIds}.");

        var ids = apprenticeshipIds.AsReadOnly();
        var checks = await repository.GetLatestChecksByApprenticeshipIds(ids);

        var checksList = checks
            .Where(c => c.ApprenticeshipId.HasValue)
            .Select(MapToEvsCheck)
            .ToList();

        return Ok(new GetEmploymentChecksResponse { Checks = checksList });
    }

    private static EvsCheckResponse MapToEvsCheck(ApplicationModels.EmploymentCheck c)
    {
        return new EvsCheckResponse
        {
            EmployerId = c.AccountId,
            ApprenticeshipId = c.ApprenticeshipId.Value,
            Uln = c.Uln.ToString(),
            RequestDate = c.CreatedOn,
            DateOfCheck = c.LastUpdatedOn,
            Result = new EvsCheckResult
            {
                Employed = c.Employed,
                CompletionStatus = c.RequestCompletionStatus,
                ErrorCode = c.ErrorType
            }
        };
    }
}
