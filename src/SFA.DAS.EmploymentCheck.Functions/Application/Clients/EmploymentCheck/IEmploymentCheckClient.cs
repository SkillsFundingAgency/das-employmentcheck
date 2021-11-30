using SFA.DAS.EmploymentCheck.Functions.Application.Models.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<Models.Domain.EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client(long employmentCheckLastGetId);

        Task EnqueueApprenticeEmploymentCheckMessages_Client(EmploymentCheckData apprenticeEmploymentData);

        Task<EmploymentCheckMessage> DequeueApprenticeEmploymentCheckMessage_Client();

        Task SaveApprenticeEmploymentCheckResult_Client(EmploymentCheckMessage apprenticeEmploymentCheckMessageModel);
    }
}
