using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Mediators.Queries.GetPayeSchemes
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