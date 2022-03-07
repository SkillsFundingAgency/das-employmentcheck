using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.ApiClients
{
    public interface IGetApiClient<T>
    {
        Task<TResponse> Get<TResponse>(IGetApiRequest request);
        Task<HttpResponseMessage> Get(IGetApiRequest request);
    }
}
