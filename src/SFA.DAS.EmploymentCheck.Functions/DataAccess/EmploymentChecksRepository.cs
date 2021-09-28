using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Dtos;
using SFA.DAS.EmploymentCheck.Functions.Models;

namespace SFA.DAS.EmploymentCheck.Functions.DataAccess
{
    public class EmploymentChecksRepository : IEmploymentChecksRepository
    {
        private readonly string _connectionString;

        public EmploymentChecksRepository(ApplicationSettings applicationSettings)
        {
            _connectionString = applicationSettings.DbConnectionString;
        }

        public async Task<List<ApprenticeToVerifyDto>> GetApprenticesToCheck()
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.QueryAsync<EmploymentCheckResult>(
                    sql: "SELECT * FROM [dbo].[EmploymentChecks]",
                    commandType: CommandType.Text);

                return result.Select(x => new ApprenticeToVerifyDto(x.Id, x.AccountId, x.NationalInsuranceNumber, x.ULN, x.UKPRN, x.ApprenticeshipId, x.MinDate, x.MaxDate)).ToList();
            }
        }

        public async Task SaveEmploymentCheckResult(long id, bool result)
        {
            await using (var connection = new SqlConnection(_connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@id", id);
                parameters.Add("@result", result);

                await connection.OpenAsync();
                await connection.ExecuteAsync(
                    sql: "UPDATE dbo.EmploymentChecks SET Result = @result WHERE Id = @id",
                    commandType: CommandType.Text,
                    param: parameters);
            }
        }
    }
}
