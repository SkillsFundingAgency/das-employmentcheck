using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Data.Repositories
{
    public interface IEmploymentCheckRepository
    {
        Task Save(Models.EmploymentCheck employmentCheck);
        public Task<Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId);
        public Task Insert(Models.EmploymentCheck employmentCheck);
    }
}
