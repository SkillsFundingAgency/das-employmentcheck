using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Clients
{
    public interface ILearnersApiClient
    {
        Task<TResponse> Get<TResponse>(string url);
    }
}
