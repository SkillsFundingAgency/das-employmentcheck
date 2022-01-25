using System.Collections.Generic;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
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