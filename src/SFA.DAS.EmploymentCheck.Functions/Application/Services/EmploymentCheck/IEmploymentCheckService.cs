using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        /// <inheritdoc>
        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database.
        /// </summary>
        /// <returns>IList<EmploymentCheckModel></returns>
        Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecksBatch_Service(long employmentCheckLastHighestBatchId);

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue.
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        Task EnqueueEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        Task EnqueueEmploymentCheckMessages_Service(
                    ILogger logger,
                    string connectionString,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider,
                    EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue.
        /// </summary>
        /// <returns></returns>
        Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<EmploymentCheckMessage></returns>
        Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Base(
                    ILogger logger,
                    string connectionString,
                    int batchSize,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider);

        Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage employmentCheckMessage);

        Task SeedEmploymentCheckDatabaseTableTestData();

        Task SeedEmploymentCheckDatabaseTableTestData(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider);

    }
}
