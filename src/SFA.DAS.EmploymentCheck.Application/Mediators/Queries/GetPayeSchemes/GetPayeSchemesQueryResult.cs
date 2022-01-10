using SFA.DAS.EmploymentCheck.Domain.Entities;
using System.Collections.Generic;

namespace SFA.DAS.EmploymentCheck.Application.Mediators.Queries.GetPayeSchemes
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