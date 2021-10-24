using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetEmployerPayeSchemes
{
    public class GetEmployersPayeSchemesMediatorResult
    {
        public GetEmployersPayeSchemesMediatorResult(IList<EmployerPayeSchemes> employersPayeSchemes)
        {
            EmployersPayeSchemes = employersPayeSchemes;
        }

        public IList<EmployerPayeSchemes> EmployersPayeSchemes { get; set; }
    }
}