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

            if (employmentCheckData.EmployerPayeSchemes == null)
            {
                return PayeFailure;
            }

            if (!employmentCheckData.EmployerPayeSchemes.PayeSchemes.Any())
            {
                return PayeNotFound;
            }
            else
            {
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
                foreach (var employerPayeScheme in employmentCheckData.EmployerPayeSchemes.PayeSchemes)
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions
                {
                    if(string.IsNullOrEmpty(employerPayeScheme))
                    {
                        return PayeNotFound;
                    }
                }
            }

            var httpStatusCode = employmentCheckData.EmployerPayeSchemes.HttpStatusCode;
            if (httpStatusCode == HttpStatusCode.NotFound || httpStatusCode == HttpStatusCode.NoContent)
            {
                return PayeNotFound;
            }

            if ((int)httpStatusCode >= 400 && (int)httpStatusCode <= 599)
            {
                return PayeFailure;
            }

            return null;
        }
    }
}
