using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureServiceTokenProvider
{
    public class TokenProvider : ITokenProvider
    {
        public virtual async Task<string> GetTokenAsync(string resource)
        {
            var azureServiceTokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
            string token = await azureServiceTokenProvider.GetAccessTokenAsync(resource);
            return token;
        }
    }
}

