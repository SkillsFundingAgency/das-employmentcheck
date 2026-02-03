using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.EmploymentCheck.Api.Application.Helpers;
using SFA.DAS.EmploymentCheck.Api.Configuration;
using Models = SFA.DAS.EmploymentCheck.Api.Application.Models;

namespace SFA.DAS.EmploymentCheck.Api.Repositories;

public class EmploymentCheckRepository(
    EmploymentCheckSettings applicationSettings,
    AzureServiceTokenProvider azureServiceTokenProvider = null)
    : IEmploymentCheckRepository
{
    private readonly string _connectionString = applicationSettings.DbConnectionString;

    public async Task<Models.EmploymentCheck> GetEmploymentCheck(Guid correlationId)
    {
        var dbConnection = new DbConnection();
        await using var sqlConnection = await dbConnection.CreateSqlConnection(
            _connectionString,
            azureServiceTokenProvider);

        var lastCheck = (await sqlConnection.GetAllAsync<Application.Models.EmploymentCheck>())
            .FirstOrDefault(x => x.CorrelationId == correlationId);

        return lastCheck;
    }

    public async Task Insert(Models.EmploymentCheck employmentCheck)
    {
        var dbConnection = new DbConnection();
        await using var sqlConnection = await dbConnection.CreateSqlConnection(
            _connectionString,
            azureServiceTokenProvider);

        await sqlConnection.InsertAsync(employmentCheck);
    }

    public async Task<IReadOnlyList<Models.EmploymentCheck>> GetLatestChecksByApprenticeshipIds(IReadOnlyList<long> apprenticeshipIds)
    {
        if (apprenticeshipIds == null || apprenticeshipIds.Count == 0)
            return [];

        var dbConnection = new DbConnection();
        await using var sqlConnection = await dbConnection.CreateSqlConnection(
            _connectionString,
            azureServiceTokenProvider);

        const string sql = @"
                WITH Ranked AS (
                    SELECT Id, CorrelationId, CheckType, Uln, ApprenticeshipId, AccountId, MinDate, MaxDate,
                           Employed, RequestCompletionStatus, ErrorType, CreatedOn, LastUpdatedOn,
                           ROW_NUMBER() OVER (PARTITION BY ApprenticeshipId ORDER BY Id DESC) AS rn
                    FROM [Business].[EmploymentCheck]
                    WHERE ApprenticeshipId IN @ApprenticeshipIds
                      AND RequestCompletionStatus = 2
                )
                SELECT Id, CorrelationId, CheckType, Uln, ApprenticeshipId, AccountId, MinDate, MaxDate,
                       Employed, RequestCompletionStatus, ErrorType, CreatedOn, LastUpdatedOn
                FROM Ranked
                WHERE rn = 1";

        var list = (await sqlConnection.QueryAsync<Models.EmploymentCheck>(sql, new { ApprenticeshipIds = apprenticeshipIds }))
            .ToList();
        return list;
    }
}