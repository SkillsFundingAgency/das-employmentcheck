using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Services.Hmrc
{
    public interface IHmrcTokenStore
    {
        Task<string> GetTokenAsync(bool forceRefresh = false);
    }
}