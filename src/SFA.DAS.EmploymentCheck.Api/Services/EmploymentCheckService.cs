using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Services
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        public async Task<Functions.Application.Models.EmploymentCheck> CheckForExistingEmploymentCheck(Guid correlationId)
        {
            throw new NotImplementedException();
        }

        public void InsertEmploymentCheck(Functions.Application.Models.EmploymentCheck employmentCheck)
        {
            throw new NotImplementedException();
        }

        public int GetLastId()
        {
            throw new NotImplementedException();
        }
    }
}