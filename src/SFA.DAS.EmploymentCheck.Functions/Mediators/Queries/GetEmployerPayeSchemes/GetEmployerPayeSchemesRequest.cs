using MediatR;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployerPayeSchemesRequest
        : IRequest<GetEmployerPayeSchemesResult>
    {
        public GetEmployerPayeSchemesRequest(IList<EmployerPayeSchemesDto> employerPayeSchemesDtos)
        {
            EmployerPayeSchemesDtos = employerPayeSchemesDtos;
        }

        public IList<EmployerPayeSchemesDto> EmployerPayeSchemesDtos { get; }
    }
}

