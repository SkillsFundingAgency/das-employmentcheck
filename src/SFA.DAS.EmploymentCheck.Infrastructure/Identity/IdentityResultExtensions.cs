using Microsoft.AspNetCore.Identity;
using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System.Linq;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Identity
{
    public static class IdentityResultExtensions
    {
        public static Result ToApplicationResult(this IdentityResult result)
        {
            return result.Succeeded
                ? Result.Success()
                : Result.Failure(result.Errors.Select(e => e.Description));
        }
    }
}