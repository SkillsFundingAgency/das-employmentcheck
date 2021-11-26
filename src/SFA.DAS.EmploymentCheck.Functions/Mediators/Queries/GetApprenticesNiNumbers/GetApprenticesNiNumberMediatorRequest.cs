using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumberMediatorRequest
        : IRequest<GetApprenticesNiNumberMediatorResult>
    {
        public GetApprenticesNiNumberMediatorRequest(
            IList<EmploymentCheckModel> apprenticeEmploymentCheck)
        {
            ApprenticeEmploymentCheck = apprenticeEmploymentCheck;
        }

        public IList<EmploymentCheckModel> ApprenticeEmploymentCheck { get; }
    }
}
