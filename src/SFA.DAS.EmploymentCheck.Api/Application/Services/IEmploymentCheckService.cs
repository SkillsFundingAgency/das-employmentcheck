using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Application.Services
{
    public interface IEmploymentCheckService
    {
        public Task<Models.EmploymentCheck> GetLastEmploymentCheck(Guid correlationId);
        public Task InsertEmploymentCheck(Models.EmploymentCheck employmentCheck);
    }
}