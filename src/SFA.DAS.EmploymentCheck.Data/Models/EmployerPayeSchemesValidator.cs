using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemesValidator : IEmployerPayeSchemesValidator
    {
        public string PayeSchemesHasError(EmploymentCheckData employmentCheckData)
        {
            const string PayeNotFound = "PAYENotFound";
            const string PayeFailure = "PAYEFailure";

            if (employmentCheckData.EmployerPayeSchemes?.HttpStatusCode == HttpStatusCode.NotFound)
            {
                return PayeNotFound;
            }

            if(employmentCheckData.EmployerPayeSchemes != null)
            {
#pragma warning disable S1066 // Collapsible "if" statements should be merged
                if ((int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode >= 400 && (int)employmentCheckData.EmployerPayeSchemes.HttpStatusCode <= 599)
#pragma warning restore S1066 // Collapsible "if" statements should be merged
                {
                    return PayeFailure;
                }
            }

            if (employmentCheckData.EmployerPayeSchemes?.PayeSchemes == null || !employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                return PayeNotFound;
            }

            return null;
        }
    }
}
