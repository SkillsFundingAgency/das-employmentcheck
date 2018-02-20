
using Dapper;
using SFA.DAS.EmploymentCheck.Domain.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.AcceptanceTests.Infrastructure
{
    public class EmploymentCheckRepository : DbConnection
    {
        public EmploymentCheckRepository(string connectionString) : base(connectionString)
        {

        }

        public async Task RemoveSubmissionEvents()
        {
            await WithConnection(async c => await c.ExecuteAsync(
                sql: "delete from employer_check.[DAS_SubmissionEvents]",
                commandType: CommandType.Text));
        }

        public async Task SetLastProcessedEventId()
        {
            await WithConnection(async c => await c.ExecuteAsync(
                sql: "update employer_check.[LastProcessedEvent] set Id = 0",
                commandType: CommandType.Text));
        }

        public async Task<long> GetLastProcessedEventId()
        {
            var result = await WithConnection(async c => await c.ExecuteScalarAsync<long>(
                sql: "[employer_check].[GetLastKnownProcessedEventId]",
                commandType: CommandType.StoredProcedure));

            return result;
        }

        public async Task<List<PreviousHandledSubmissionEvent>> GetPreviouslyHandledSubmissionEvents(long uln)
        {
            var result = await WithConnection(async c =>
            {
                return await c.QueryAsync<PreviousHandledSubmissionEvent>(
                    sql: $"select * from [employer_check].[DAS_SubmissionEvents] where Uln = {uln}",
                    commandType: CommandType.Text);
            }
            );

            return result.ToList();
        }
    }
}
