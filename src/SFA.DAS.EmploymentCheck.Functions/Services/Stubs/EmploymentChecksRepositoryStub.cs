using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Services.Fakes
{
    public class EmploymentChecksRepositoryStub : IEmploymentChecksRepository
    {
        private IRandomNumberService _randomNumberService;
        private readonly ILogger<IEmploymentChecksRepository> _logger;

        public EmploymentChecksRepositoryStub(
            IRandomNumberService randomNumberService,
            ILogger<IEmploymentChecksRepository> logger)
        {
            _randomNumberService = randomNumberService;
            _logger = logger;
        }

        public async Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck()
        {
            var thisMethodName = "***** FakeEmploymentChecksRepository.GetApprenticesToCheck() *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            var learners = new List<ApprenticeToVerifyDto>();
            for (int i = 0; i < 1; i++)
            {
                var learner = new ApprenticeToVerifyDto()
                {
                    Id = i,
                    AccountId = 1,
                    NationalInsuranceNumber = "12345678",
                    ULN = 100000001,
                    UKPRN = 10000001,
                    ApprenticeshipId = 10,
                    StartDate = new DateTime(2021, 1, 1),
                    EndDate = new DateTime(2021, 1, 1)
                };

                learners.Add(learner);
            }

            _logger.LogInformation($"{messagePrefix} ***** GetApprenticesToCheck()] returned {learners.Count} apprentices. *****");
            return await Task.FromResult(learners);
        }

        public Task SaveEmploymentCheckResult(long id, bool result)
        {
            throw new NotImplementedException();
        }
    }
}
