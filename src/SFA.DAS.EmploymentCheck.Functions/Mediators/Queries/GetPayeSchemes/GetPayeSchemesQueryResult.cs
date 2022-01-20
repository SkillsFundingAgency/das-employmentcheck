using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryResult
    {
        public GetPayeSchemesQueryResult(IList<EmployerPayeSchemes> employersPayeSchemes)
        {
            EmployersPayeSchemes = employersPayeSchemes;
        }

        public IList<EmployerPayeSchemes> EmployersPayeSchemes { get; set; }
    }
}