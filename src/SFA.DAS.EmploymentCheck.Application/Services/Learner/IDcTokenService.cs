using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Data.Models;

namespace SFA.DAS.EmploymentCheck.Application.Services.Learner
{
    public interface IDcTokenService
    {
        Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope);
    }
}