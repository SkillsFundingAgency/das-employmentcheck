using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.SubmitLearnerData
{
    public interface ISubmitLearnerDataClient
    {
        Task<IList<ApprenticeNiNumber>> GetApprenticesNiNumber(IList<ApprenticeEmploymentCheckModel> apprentices);
    }
}
