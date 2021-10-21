using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployerPayeSchemesResult
    {
        public GetEmployerPayeSchemesResult(IList<EmployerPayeSchemesDto> employerPayeSchemesDtos)
        {
            EmployerPayeSchemesDtos = employerPayeSchemesDtos;
        }

        public IList<EmployerPayeSchemesDto> EmployerPayeSchemesDtos { get; set; }
    }
}