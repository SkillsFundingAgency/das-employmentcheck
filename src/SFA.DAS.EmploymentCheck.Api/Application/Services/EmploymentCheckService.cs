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

        public async Task<Models.EmploymentCheck> CheckForExistingEmploymentCheck(Guid correlationId)
        {
            var existingEmploymentCheck = await _employmentCheckRepository.GetEmploymentCheck(correlationId);

            return existingEmploymentCheck;
        }

        public void InsertEmploymentCheck(Models.EmploymentCheck employmentCheck)
        {
            _employmentCheckRepository.Insert(employmentCheck);
        }

        public async Task<int> GetLastId()
        {
            var lastId = await _employmentCheckRepository.GetLastId();

            return lastId;
        }
    }
}