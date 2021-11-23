using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Services
{
    public interface IOAuthTokenService
    {
        Task<OAuthAccessToken> GetAccessToken(string clientSecret);
        string TotpSecret { get; }
    }
}