using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Api.Application.Services
{
    public interface IEmploymentCheckService
    {
        public Task<Functions.Application.Models.EmploymentCheck> CheckForExistingEmploymentCheck(Guid correlationId);
        public void InsertEmploymentCheck(Functions.Application.Models.EmploymentCheck employmentCheck);
        public Task<int> GetLastId();
    }
}