using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Data
{
    public class SubmissionEventRepository : BaseRepository, ISubmissionEventRepository
    {
        private readonly IConfiguration _configuration;

        public SubmissionEventRepository(IConfiguration configuration, ILog logger) : base(configuration.DatabaseConnectionString, logger)
        {
            _configuration = configuration;
        }

        public async Task<long> GetPollingProcessingStartingPoint()
        {
            var result = await WithConnection(async c => await c.ExecuteAsync(
                sql: "[employer_check].[GetLastKnownProcessedEventId]",
                commandType: CommandType.StoredProcedure));

            return result;
        }
    }
}
