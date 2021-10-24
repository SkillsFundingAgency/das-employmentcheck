using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetApprenticesNiNumbers
{
    public class GetApprenticesNiNumberMediatorRequest
        : IRequest<GetApprenticesNiNumberMediatorResult>
    {
        public GetApprenticesNiNumberMediatorRequest(IList<Apprentice> apprentices)
        {
            Apprentices = apprentices;
        }

        public IList<Apprentice> Apprentices { get; }
    }
}
