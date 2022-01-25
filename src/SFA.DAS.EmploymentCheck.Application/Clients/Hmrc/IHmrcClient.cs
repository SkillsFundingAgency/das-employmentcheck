using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Clients.Hmrc
{
    public interface IHmrcClient
    {
        Task<EmploymentCheckCacheRequest> CheckEmploymentStatus(EmploymentCheckCacheRequest employmentCheckCacheRequest);
    }
}
