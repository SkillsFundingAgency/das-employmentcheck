using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<Models.Domain.EmploymentCheckModel>> GetEmploymentChecksBatch_Client(long employmentCheckLastHighestBatchId);

        Task EnqueueEmploymentCheckMessages_Client(EmploymentCheckData employmentCheckData);

        Task<EmploymentCheckMessage> DequeueEmploymentCheckMessage_Client();

        Task SaveEmploymentCheckResult_Client(EmploymentCheckMessage employmentCheckMessage);
    }
}
