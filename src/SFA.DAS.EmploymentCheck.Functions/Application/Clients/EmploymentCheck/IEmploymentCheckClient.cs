using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<EmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client(long employmentCheckLastGetId);

        Task EnqueueApprenticeEmploymentCheckMessages_Client(EmploymentCheckData apprenticeEmploymentData);

        Task<EmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Client();

        Task SaveApprenticeEmploymentCheckResult_Client(EmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);
    }
}
