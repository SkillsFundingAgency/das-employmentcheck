using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.Learner
{
    public interface IDcTokenService
    {
        Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope);
    }
}