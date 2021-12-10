using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployersPayeSchemesMediatorRequest
        : IRequest<GetEmployersPayeSchemesMediatorResult>
    {
        public GetEmployersPayeSchemesMediatorRequest(IList<ApprenticeEmploymentCheckModel> apprentices)
        {
            Apprentices = apprentices;
        }

        public IList<ApprenticeEmploymentCheckModel> Apprentices { get; }
    }
}

