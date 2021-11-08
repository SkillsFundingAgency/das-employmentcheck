using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Services;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public class EmploymentCheckServiceStub : IEmploymentCheckService
    {
        private IRandomNumberService _randomNumberService;
        private readonly ILoggerAdapter<IEmploymentCheckService> _logger;

        private readonly string _connectionString =
            System.Environment.GetEnvironmentVariable($"EmploymentChecksConnectionString");

        public EmploymentCheckServiceStub(
            IRandomNumberService randomNumberService,
            ILoggerAdapter<IEmploymentCheckService> logger)
        {
            _randomNumberService = randomNumberService;
            _logger = logger;
        }

        public  async Task<int> SaveEmploymentCheckResult(
            long id,
            bool result)
        {
            throw new NotImplementedException("METHOD NOT IN USE, USE OTHER METHOD SIGNATURE: SaveEmploymentCheckResult(long id, long uln, bool result)");
        }

        public async Task<int> SaveEmploymentCheckResult(long id, long uln, bool result)
        {
            var thisMethodName = "EmploymentCheckServiceStub.SaveEmploymentCheckResult()";

            Log.WriteLog(_logger, thisMethodName, $"Starting employment check save for ULN: [{uln}].");

            var parameters = new DynamicParameters();

            parameters.Add("id", id, DbType.Int64);
            parameters.Add("result", result, DbType.Boolean);
            parameters.Add("checked", true);

            var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync();

            Log.WriteLog(_logger, thisMethodName, $"Updating row for ULN: {uln}.");
            return await connection.ExecuteAsync(
                "UPDATE [dbo].[EmploymentChecks] SET IsEmployed = @result, LastUpdated = GETDATE(), HasBeenChecked = @checked WHERE Id = @id",
                    parameters,
                    commandType: CommandType.Text);
        }

        //public async Task<List<Apprentice>> GetApprentices(SqlConnection sqlConnection)
        //{
        //    var thisMethodName = "EmploymentCheckServiceStub.GetLearnersRequiringEmploymentChecks()";
        //    List<Apprentice> learnersRequiringEmploymentCheckDto = null;

        //    await sqlConnection.OpenAsync();

        //    var result = await sqlConnection.QueryAsync<EmploymentCheckResult>(
        //        "SELECT TOP 5 * FROM [dbo].[EmploymentChecks] WHERE HasBeenChecked = 0 ORDER BY CreatedDate",
        //        commandType: CommandType.Text);

        //    if (result != null && result.Any())
        //    {
        //        learnersRequiringEmploymentCheckDto = result.Select(x => new Apprentice(
        //            x.Id,
        //            x.AccountId,
        //            x.NationalInsuranceNumber,
        //            x.ULN,
        //            x.UKPRN,
        //            x.ApprenticeshipId,
        //            x.MinDate,
        //            x.MaxDate)).ToList();
        //        Log.WriteLog(_logger, thisMethodName, $"Database query returned {learnersRequiringEmploymentCheckDto.Count} learners.");
        //    }
        //    else
        //    {
        //        Log.WriteLog(_logger, thisMethodName, $"Database query returned [0] learners.");
        //        learnersRequiringEmploymentCheckDto = new List<Apprentice>(); // return an empty list rather than null
        //    }

        //    return learnersRequiringEmploymentCheckDto;
        //}

        public async Task<IList<Apprentice>> GetApprentices()
        {
            var apprentices = new List<Apprentice>
            {
                new Apprentice
                {
                    Id = 1,
                    AccountId = 1,
                    NationalInsuranceNumber = "1",
                    ULN = 1,
                    UKPRN = 1,
                    ApprenticeshipId = 1,
                    StartDate = new DateTime(2021, 10, 1),
                    EndDate = new DateTime(2021, 10, 2)
                }
            };

            return await Task.FromResult(apprentices);
        }
    }
}
