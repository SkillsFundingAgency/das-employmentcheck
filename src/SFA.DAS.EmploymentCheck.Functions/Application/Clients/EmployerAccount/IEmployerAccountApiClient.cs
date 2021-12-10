using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount
{
    public interface IEmployerAccountApiClient
    {
        Task<TResponse> Get<TResponse>(string url);
    }
}
