using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.ApiClients
{
    public abstract class GetApiClient<T> : IEmployerAccountApiClient<T> where T : IApiConfiguration
    {
        protected readonly HttpClient HttpClient;
        protected readonly IWebHostEnvironment HostingEnvironment;
        protected readonly T Configuration;

        public GetApiClient(
            IHttpClientFactory httpClientFactory,
            T apiConfiguration,
            IWebHostEnvironment hostingEnvironment)
        {
            HttpClient = httpClientFactory.CreateClient();
            HttpClient.BaseAddress = new Uri(apiConfiguration.Url);
            HostingEnvironment = hostingEnvironment;
            Configuration = apiConfiguration;
        }

        public async Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            var response = await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseBody = JsonConvert.DeserializeObject<TResponse>(json);

            return responseBody;
        }

        public async Task<HttpResponseMessage> Get(IGetApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            
            await AddAuthenticationHeader(httpRequestMessage);

            var response = await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            return response;
        }

        protected abstract Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage);
    }
}
