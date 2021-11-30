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
        Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service(long employmentCheckLastGetId);

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue.
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        Task EnqueueApprenticeEmploymentCheckMessages_Service(EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        Task EnqueueApprenticeEmploymentCheckMessages_Service(
                    ILogger logger,
                    string connectionString,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider,
                    EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API.
        /// </summary>
        /// <returns></returns>
        Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Base(
                    ILogger logger,
                    string connectionString,
                    int batchSize,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider);

        Task SaveEmploymentCheckResult_Service(EmploymentCheckMessage employmentCheckMessage);

        Task SeedEmploymentCheckApprenticeDatabaseTableTestData();

        Task SeedEmploymentCheckApprenticeDatabaseTableTestData(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider);

    }
}
