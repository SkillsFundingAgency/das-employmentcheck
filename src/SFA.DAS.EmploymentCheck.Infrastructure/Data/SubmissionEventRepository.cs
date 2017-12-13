using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Models;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Data
{
    public class SubmissionEventRepository : BaseRepository, ISubmissionEventRepository
    {
        public SubmissionEventRepository(IConfiguration configuration, ILog logger) : base(configuration.DatabaseConnectionString, logger)
        {
        }

        public async Task<long> GetPollingProcessingStartingPoint()
        {
            var result = await WithConnection(async c => await c.ExecuteAsync(
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
