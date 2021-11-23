using System.Threading.Tasks;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Services.SubmitLearnerData
{
    public interface IDcTokenService
    {
        Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope);
    }
}