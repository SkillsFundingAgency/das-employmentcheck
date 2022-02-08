using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.AzureServiceTokenProvider
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync(string resource);
    }
}
