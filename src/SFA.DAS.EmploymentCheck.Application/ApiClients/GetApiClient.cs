using Boxed.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Polly.Wrap;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.ApiClients
{
    public abstract class GetApiClient<T> : IGetApiClient<T> where T : IApiConfiguration
    {
        protected readonly HttpClient HttpClient;
        protected readonly IWebHostEnvironment HostingEnvironment;
        protected readonly T Configuration;

        protected GetApiClient(
            IHttpClientFactory httpClientFactory,
            T apiConfiguration,
            IWebHostEnvironment hostingEnvironment)
        {
            HttpClient = httpClientFactory.CreateClient();
            HttpClient.BaseAddress = new Uri(apiConfiguration.BaseUrl);
            HostingEnvironment = hostingEnvironment;
            Configuration = apiConfiguration;
        }

        public async Task<HttpResponseMessage> Get(IGetApiRequest request)
        {
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, request.GetUrl);
            
            await AddAuthenticationHeader(httpRequestMessage);

            var response = await HttpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);

            return response;
        }

        public async Task<HttpResponseMessage> GetWithPolicy(AsyncPolicyWrap policy, IGetApiRequest request)
        {
            HttpResponseMessage response = null;

            try
            {
                await policy.ExecuteAsync(async () =>
                {
                    response = await Get(request);
                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        throw new HttpException(response.StatusCode);
                    }

                });

                return response;
            }
            catch (HttpException)
            {
                return response;
            }
        }

        protected abstract Task AddAuthenticationHeader(HttpRequestMessage httpRequestMessage);
    }
}
