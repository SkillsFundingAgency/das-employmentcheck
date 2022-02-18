using System;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Api.Repositories;

namespace SFA.DAS.EmploymentCheck.Api.Application.Services
{
    public class EmploymentCheckService : IEmploymentCheckService
    {
        private readonly IEmploymentCheckRepository _employmentCheckRepository;

        public EmploymentCheckService(IEmploymentCheckRepository employmentCheckRepository)
        {
            _employmentCheckRepository = employmentCheckRepository;
        }

        public async Task<Models.EmploymentCheck> GetLastEmploymentCheck(Guid correlationId)
        {
            var existingEmploymentCheck = await _employmentCheckRepository.GetEmploymentCheck(correlationId);

            return existingEmploymentCheck;
        }

        public async Task InsertEmploymentCheck(Models.EmploymentCheck employmentCheck)
        {
            await _employmentCheckRepository.Insert(employmentCheck);
        }
    }
}