using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EmploymentCheck.DataAccess
{
    public class SubmissionEventRepository : BaseRepository, ISubmissionEventRepository
    {
        public SubmissionEventRepository(IEmploymentCheckConfiguration configuration, ILog logger) : base(configuration.DatabaseConnectionString, logger)
        {
        }

        public async Task<long> GetLastProcessedEventId()
        {
            var result = await WithConnection(async c => await c.ExecuteScalarAsync<long>(
                sql: "[employer_check].[GetLastKnownProcessedEventId]",
                commandType: CommandType.StoredProcedure));

            return result;
        }

        public async Task<IEnumerable<PreviousHandledSubmissionEvent>> GetPreviouslyHandledSubmissionEvents(IEnumerable<long> ulns)
        {
            var result = await WithConnection(async c =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@ulns", GenerateUlnsDataTable(ulns).AsTableValuedParameter("employer_check.UlnTableType"));

                    return await c.QueryAsync<PreviousHandledSubmissionEvent>(
                        sql: "[employer_check].[GetPreviousHandledSubmissionEvents]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure);
                }
            );

            return result;
        }

        public async Task SetLastProcessedEvent(long id)
        {
            await WithConnection(async c =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@lastEventId", id);

                    return await c.ExecuteAsync(
                        sql: "[employer_check].[SetLastProcessedEventId]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure);
                }
            );
        }

        public async Task StoreEmploymentCheckResult(PreviousHandledSubmissionEvent submissionEvent)
        {
            await WithConnection(async c =>
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@uln", submissionEvent.Uln);
                    parameters.Add("@nationalInsuranceNumber", submissionEvent.NiNumber);
                    parameters.Add("@passedValidationCheck", submissionEvent.PassedValidationCheck);

                    return await c.ExecuteAsync(
                        sql: "[employer_check].[StoreEmploymentCheckResult]",
                        param: parameters,
                        commandType: CommandType.StoredProcedure);
                }
            );
        }

        private DataTable GenerateUlnsDataTable(IEnumerable<long> ulns)
        {
            var result = new DataTable();

            result.Columns.Add("Uln", typeof(long));

            foreach (var uln in ulns)
            {
                var row = result.NewRow();
                row["Uln"] = uln;
                result.Rows.Add(row);
            }

            return result;
        }
    }
}
