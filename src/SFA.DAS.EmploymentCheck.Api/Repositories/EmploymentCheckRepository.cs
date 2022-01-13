using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Repositories
{
    public class EmploymentCheckRepository : IEmploymentCheckRepository
    {
        public async Task<Functions.Application.Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId)
        {
            throw new NotImplementedException();
        }

        public void Insert(Functions.Application.Models.EmploymentCheck employmentCheck)
        {
            throw new NotImplementedException();
        }

        public async Task<int> GetLastId()
        {
            throw new NotImplementedException();
        }
    }
}