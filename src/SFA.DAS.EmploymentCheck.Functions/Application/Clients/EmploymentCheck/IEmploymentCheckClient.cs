using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck
{
    public interface IEmploymentCheckClient
    {
        Task<IList<ApprenticeEmploymentCheckModel>> GetApprenticeEmploymentChecksBatch_Client();

        Task EnqueueApprenticeEmploymentCheckMessages_Client(ApprenticeRelatedData apprenticeEmploymentData);

        Task<ApprenticeEmploymentCheckMessageModel> DequeueApprenticeEmploymentCheckMessage_Client();

        Task SaveApprenticeEmploymentCheckResult_Client(ApprenticeEmploymentCheckMessageModel apprenticeEmploymentCheckMessageModel);
    }
}
