using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumberMediatorRequest
        : IRequest<GetApprenticesNiNumberMediatorResult>
    {
        public GetApprenticesNiNumberMediatorRequest(
            IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentCheck)
        {
            ApprenticeEmploymentCheck = apprenticeEmploymentCheck;
        }

        public IList<ApprenticeEmploymentCheckModel> ApprenticeEmploymentCheck { get; }
    }
}
