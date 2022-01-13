using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Dapper;
using SFA.DAS.EmploymentCheck.Functions.Application.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Repositories
{
    public interface IEmploymentCheckCacheResponseRepository
    {
        Task<int> Save(EmploymentCheckCacheResponse employmentCheckCacheResponse);
    }

    public class EmploymentCheckCacheResponseRepository
    {
        public async Task<int> Save(
          EmploymentCheckCacheResponse employmentCheckCacheResponse)
        {
            if (employmentCheckCacheResponse == null) return await Task.FromResult(0);

            var dbConnection = new DbConnection();
            await using (var sqlConnection = await dbConnection.CreateSqlConnection(
                _connectionString,
                AzureResource,
                _azureServiceTokenProvider))
            {
                Guard.Against.Null(sqlConnection, nameof(sqlConnection));

                await sqlConnection.OpenAsync();
                var parameter = new DynamicParameters();
                parameter.Add("@apprenticeEmploymentCheckId", employmentCheckCacheResponse.ApprenticeEmploymentCheckId, DbType.Int64);
                parameter.Add("@employmentCheckCacheRequestId", employmentCheckCacheResponse.EmploymentCheckCacheRequestId, DbType.Int64);
                parameter.Add("@correlationId", employmentCheckCacheResponse.CorrelationId, DbType.Guid);
                parameter.Add("@employed", employmentCheckCacheResponse.Employed, DbType.Boolean);
                parameter.Add("@foundOnPaye", employmentCheckCacheResponse.FoundOnPaye, DbType.String);
                parameter.Add("@processingComplete", employmentCheckCacheResponse.ProcessingComplete, DbType.Boolean);
                parameter.Add("@count", employmentCheckCacheResponse.Count, DbType.Int32);
                parameter.Add("@httpResponse", employmentCheckCacheResponse.HttpResponse, DbType.String);
                parameter.Add("@httpStatusCode", employmentCheckCacheResponse.HttpStatusCode, DbType.Int16);
                parameter.Add("@createdOn", DateTime.Now, DbType.DateTime);

                await sqlConnection.ExecuteScalarAsync(
                    "INSERT [Cache].[EmploymentCheckCacheResponse] " +
                    "       ( ApprenticeEmploymentCheckId,  EmploymentCheckCacheRequestId,  CorrelationId,  Employed,  FoundOnPaye,  ProcessingComplete, count,   httpResponse,  HttpStatusCode,  CreatedOn) " +
                    "VALUES (@apprenticeEmploymentCheckId, @EmploymentCheckCacheRequestId, @correlationId, @employed, @foundOnPaye, @processingComplete, @count, @httpResponse, @httpStatusCode, @createdOn)",
                    parameter,
                    commandType: CommandType.Text);
            }

            return await Task.FromResult(0);
        }
    }
}
