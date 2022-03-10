using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Queries.GetPayeSchemes
{
    public class GetPayeSchemesQueryResult
    {
        public GetPayeSchemesQueryResult(EmployerPayeSchemes employersPayeSchemes)
        {
            EmployersPayeSchemes = employersPayeSchemes;
        }

        public EmployerPayeSchemes EmployersPayeSchemes { get; set; }
    }
}