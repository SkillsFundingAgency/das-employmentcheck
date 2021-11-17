using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Logging;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        /// <inheritdoc>
        /// <summary>
        /// Gets a batch of the the apprentices requiring employment checks from the Employment Check database.
        /// </summary>
        /// <returns>IList<ApprenticeEmploymentCheckModel></returns>
        Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Service();

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue.
        /// </summary>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        Task EnqueueApprenticeEmploymentCheckMessages_Service(ApprenticeRelatedData apprenticeEmploymentData);

        /// <summary>
        /// Adds an apprentice data message representing each apprentice in the ApprenticeEmploymentChecksBatch to the HMRC API message queue.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <param name="apprenticeEmploymentData"></param>
        /// <returns>Task</returns>
        Task EnqueueApprenticeEmploymentCheckMessages_Service(
                    ILogger logger,
                    string connectionString,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider,
                    ApprenticeRelatedData apprenticeEmploymentData);

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API.
        /// </summary>
        /// <returns></returns>
        Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Service();

        /// <summary>
        /// Gets an apprentice data message from the HMRC API message queue to pass to the HMRC employment check API.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString"></param>
        /// <param name="batchSize"></param>
        /// <param name="azureResource"></param>
        /// <param name="azureServiceTokenProvider"></param>
        /// <returns>Task<ApprenticeEmploymentCheckMessageModel></returns>
        Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Base(
                    ILogger logger,
                    string connectionString,
                    int batchSize,
                    string azureResource,
                    AzureServiceTokenProvider azureServiceTokenProvider);

        Task SaveEmploymentCheckResult_Service(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);

        //Task<int> SaveEmploymentCheckResults(
        //    ILogger logger,
        //    string connectionString,
        //    string azureResource,
        //    AzureServiceTokenProvider azureServiceTokenProvider,
        //    IList<ApprenticeEmploymentCheckModel> apprenticeEmploymentCheckModels);

        Task SeedEmploymentCheckApprenticeDatabaseTableTestData();

        Task SeedEmploymentCheckApprenticeDatabaseTableTestData(
            ILogger logger,
            string connectionString,
            string azureResource,
            AzureServiceTokenProvider azureServiceTokenProvider);

    }
}
