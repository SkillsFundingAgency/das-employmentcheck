using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Repositories
{
    public interface IEmploymentCheckRepository
    {
        public Task<Functions.Application.Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId);
        public void Insert(Functions.Application.Models.EmploymentCheck employmentCheck);
        public Task<int> GetLastId();
    }
}