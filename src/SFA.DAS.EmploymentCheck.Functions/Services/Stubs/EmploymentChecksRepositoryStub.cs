using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.IdentityModel.Protocols;

namespace SFA.DAS.EmploymentCheck.Functions.Services.Fakes
{
    public class EmploymentChecksRepositoryStub : IEmploymentChecksRepository
    {
        private IRandomNumberService _randomNumberService;
        private readonly ILogger<IEmploymentChecksRepository> _logger;

        private readonly string _connectionString =
            System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString");

        public EmploymentChecksRepositoryStub(
            IRandomNumberService randomNumberService,
            ILogger<IEmploymentChecksRepository> logger)
        {
            _randomNumberService = randomNumberService;
            _logger = logger;
        }

        public async Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck()
        {
            //var thisMethodName = "***** EmploymentChecksRepositoryStub.GetApprenticesToCheck() *****";
            //var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

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

            //_logger.LogInformation($"{messagePrefix} ***** GetApprenticesToCheck()] returned {learners.Count} apprentices. *****");
            return await Task.FromResult(learners);
        }

        public async Task SaveEmploymentCheckResult(long id, bool result)
        {
            var thisMethodName = "***** FakeEmploymentChecksRepository.SaveEmploymentCheckResult() *****";
            var messagePrefix = $"{ DateTime.UtcNow } UTC { thisMethodName}:";

            _logger.LogInformation($"{messagePrefix} ***** SaveEmploymentCheckResult() for Id {id}. *****");

            var parameters = new DynamicParameters();

            parameters.Add("id", id, DbType.Int64);
            parameters.Add("result", result, DbType.Boolean);

            var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();
            await connection.ExecuteAsync(
                sql: "[employer_account].[AddPayeToAccount]",
                param: parameters,
                commandType: CommandType.StoredProcedure);
        }
    }
}
