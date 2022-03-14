using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public class EmployerPayeSchemesValidator : IEmployerPayeSchemesValidator
    {
        public (bool IsValid, string ErrorType) IsValidPayeScheme(EmploymentCheckData employmentCheckData)
        {
            const string PayeNotFound = "PAYENotFound";
            const string PayeFailure = "PAYEFailure";

            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                return (false, PayeFailure);
            }

            if (!employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                return (false, PayeNotFound);
            }

            if (employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
                {
                    if(string.IsNullOrEmpty(employerPayeScheme))
                    {
                        return (false, PayeNotFound);
                    }
                }
            }

            var httpStatusCode = employmentCheckData.EmployerPayeSchemes.HttpStatusCode;
            if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.NoContent)
            {
                return (false, PayeNotFound);
            }

            if ((int)httpStatusCode >= 400 && (int)httpStatusCode <= 599)
            {
                return (false, PayeFailure);
            }

            return (true, null);
        }
    }
}
