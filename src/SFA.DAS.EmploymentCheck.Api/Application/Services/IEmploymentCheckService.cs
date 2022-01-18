using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Application.Services
{
    public interface IEmploymentCheckService
    {
        public Task<Models.EmploymentCheck> CheckForExistingEmploymentCheck(Guid correlationId);
        public void InsertEmploymentCheck(Models.EmploymentCheck employmentCheck);
    }
}