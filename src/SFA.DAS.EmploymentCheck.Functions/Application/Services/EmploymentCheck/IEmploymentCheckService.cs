using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck
{
    public interface IEmploymentCheckService
    {
        /// <inheritdoc>
        /// <summary>
        /// Gets a batch of the apprentices requiring employment checks from the Employment Check database.
        /// </summary>
        /// <returns>IList<EmploymentCheckModel></returns>
        Task<IList<EmploymentCheckModel>> GetEmploymentChecksBatch(long employmentCheckLastHighestBatchId);

        /// <summary>
        /// Creates an EmploymentCheckCacheRequest for each employment check in the given list of employment checks
        /// </summary>
        /// <param name="employmentCheckModels"></param>
        /// <returns></returns>
        Task<IList<EmploymentCheckCacheRequest>> CreateEmploymentCheckCacheRequests(IList<EmploymentCheckModel> employmentCheckModels);

        /// <summary>
        /// Adds an employment check message to the HMRC API message queue.
        /// </summary>
        /// <param name="employmentCheckData"></param>
        /// <returns>Task</returns>
        Task EnqueueEmploymentCheckMessages(EmploymentCheckData employmentCheckData);

        /// <summary>
        /// Gets an employment check message from the HMRC API message queue.
        /// </summary>
        /// <returns></returns>
        Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage();

        Task SaveEmploymentCheckResult(EmploymentCheckMessage employmentCheckMessage);

        Task SeedEmploymentCheckDatabaseTableTestData();
    }
}
