using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;

namespace SFA.DAS.EmploymentCheck.Functions.Services
{
    public interface ILearnersService
    {
        Task<List<LearnerRequiringEmploymentCheckDto>> GetLearnersRequiringEmploymentCheck(SqlConnection sqlConnection);

        Task<LearnerNationalnsuranceNumberDto[]> GetLearnersNationalInsuranceNumbers(LearnerNationalnsuranceNumberDto[] learnersNinosDto);
    }
}
