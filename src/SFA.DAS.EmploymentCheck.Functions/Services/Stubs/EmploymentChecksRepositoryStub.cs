using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.DataAccess;
using SFA.DAS.EmploymentCheck.Functions.Models.Dtos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.IdentityModel.Protocols;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Services.Stubs
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

        public  async Task<int> SaveEmploymentCheckResult(
            long id,
            bool result)
        {
            //Left in place since stub and true implementation use the same interface
            var thisMethodName = "EmploymentChecksRepositoryStub.SaveEmploymentCheckResult()";

            Log.WriteLog(_logger, thisMethodName, $"Saving employment check result for Id: [{id}]");

            var parameters = new DynamicParameters();

            parameters.Add("id", id, DbType.Int64);
            parameters.Add("result", result, DbType.Boolean);

            var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();
            return await connection.ExecuteAsync(
                sql: "INSERT INTO [SavedEmploymentCheckResults] (Id, Result) VALUES (@id, @result)",
                param: parameters,
                commandType: CommandType.Text);
        }

        public async Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
        {
            var thisMethodName = "EmploymentChecksRepositoryStub.SaveEmploymentCheckResult()";

            Log.WriteLog(_logger, thisMethodName, $"Starting employment check save for ULN: [{uln}].");

            var parameters = new DynamicParameters();

            parameters.Add("id", id, DbType.Int64);
            parameters.Add("ULN", uln, DbType.Int64);
            parameters.Add("result", result, DbType.Boolean);

            var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            //checks if item is already saved and updates it if exists, saves doing it manually when re-running
            var exists = await connection.ExecuteScalarAsync(
                "SELECT * FROM [SavedEmploymentCheckResults] WHERE Id = @id",
                parameters,
                commandType: CommandType.Text);
            if (exists != null)
            {
                Log.WriteLog(_logger, thisMethodName, $"Updating row for ULN: {uln}.");
                return await connection.ExecuteAsync(
                    "UPDATE [SavedEmploymentCheckResults] SET Id = @id, ULN = @ULN, Result = @result WHERE Id = @id",
                    parameters,
                    commandType: CommandType.Text);
            }
            else
            {
                Log.WriteLog(_logger, thisMethodName, $"Saving row for ULN: {uln}");
                return await connection.ExecuteAsync(
                    sql: "INSERT INTO [SavedEmploymentCheckResults] (Id, ULN, Result) VALUES (@id, @ULN, @result)",
                    param: parameters,
                    commandType: CommandType.Text);
            }
        }

        public async Task<List<LearnerRequiringEmploymentCheckDto>> GetLearnersRequiringEmploymentChecks(SqlConnection connection)
        {
            var thisMethodName = "EmploymentChecksRepositoryStub.GetLearnersRequiringEmploymentChecks()";
            List<LearnerRequiringEmploymentCheckDto> learnersRequiringEmploymentCheckDto = null;

            await connection.OpenAsync();

            var result = await connection.QueryAsync<EmploymentCheckResult>(
                "SELECT TOP 5 * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
                commandType: CommandType.Text);

            if (result != null && result.Any())
            {
                learnersRequiringEmploymentCheckDto = result.Select(x => new LearnerRequiringEmploymentCheckDto(
                    x.Id,
                    x.ULN,
                    x.AccountId,
                    x.NationalInsuranceNumber,
                    x.UKPRN,
                    x.ApprenticeshipId,
                    x.MinDate,
                    x.MaxDate)).ToList();
                Log.WriteLog(_logger, thisMethodName, $"Database query returned {learnersRequiringEmploymentCheckDto.Count} learners.");
            }
            else
            {
                Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] learners.");
                learnersRequiringEmploymentCheckDto = new List<LearnerRequiringEmploymentCheckDto>(); // return an empty list rather than null
            }

            return learnersRequiringEmploymentCheckDto;
        }
    }
}
