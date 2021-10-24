using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumberMediatorResult
    {
        public GetApprenticesNiNumberMediatorResult(IList<ApprenticeNiNumber> apprenticeNiNumbers)
        {
            ApprenticesNiNumber = apprenticeNiNumbers;
        }

        public IList<ApprenticeNiNumber> ApprenticesNiNumber { get; }
    }
}
