using System.Linq;
using System.Net;

namespace SFA.DAS.EmploymentCheck.Data.Models
{
    public interface IEmploymentCheckDataValidator
    {
        (bool IsValid, string ErrorType) IsValidEmploymentCheckData(EmploymentCheckData employmentCheckData);

        (bool IsValid, string ErrorType) IsValidNino(EmploymentCheckData employmentCheckData);

        (bool IsValid, string ErrorType) IsValidPayeScheme(EmploymentCheckData employmentCheckData);
    }
}
