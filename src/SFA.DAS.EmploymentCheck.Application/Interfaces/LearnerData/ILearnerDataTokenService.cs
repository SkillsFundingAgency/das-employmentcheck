using SFA.DAS.EmploymentCheck.Application.Common.Models;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.Interfaces.LearnerData
{
    public interface ILearnerDataTokenService
    {
        Task<AuthResult> GetTokenAsync(string baseUrl, string grantType, string secret, string clientId, string scope);
    }
}