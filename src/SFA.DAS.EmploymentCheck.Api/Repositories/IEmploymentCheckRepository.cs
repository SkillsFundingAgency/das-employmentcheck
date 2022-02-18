using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Repositories
{
    public interface IEmploymentCheckRepository
    {
        public Task<Application.Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId);
        public Task Insert(Application.Models.EmploymentCheck employmentCheck);
    }
}