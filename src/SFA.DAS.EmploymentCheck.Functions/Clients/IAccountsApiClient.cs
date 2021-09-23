using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Clients
{
    public interface IAccountsApiClient
    {
        Task<TResponse> Get<TResponse>(string url);
    }
}
