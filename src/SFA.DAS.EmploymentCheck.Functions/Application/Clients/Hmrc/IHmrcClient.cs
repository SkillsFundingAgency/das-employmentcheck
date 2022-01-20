using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc
{
    public interface IHmrcClient
    {
        Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}
