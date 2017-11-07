using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SFA.DAS.EmploymentCheck.Domain;
using SFA.DAS.EmploymentCheck.Domain.Interfaces;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Http
{
    public class HttpClientWrapper : IHttpClientWrapper, IDisposable
    {
        private readonly HttpClient _httpClient;
        bool _disposed = false;

        public HttpClientWrapper() : this(null)
        {
            
        }

        public HttpClientWrapper(HttpMessageHandler handler)
        {
            _httpClient = handler == null ? new HttpClient() : new HttpClient(handler);
        }

        public async Task<HttpResponseMessage> GetAsync(Uri requestUri, string contentType = Constants.ContentTypeValue)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(contentType));
            return await _httpClient.SendAsync(requestMessage);
        }

        public async Task<T> ReadResponse<T>(HttpResponseMessage responseMessage)
        {
            responseMessage.EnsureSuccessStatusCode();

            var value = await responseMessage.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(value);
        }

        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent postContent, IEnumerable<KeyValuePair<string, string>> headers)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _httpClient.Dispose();
            }

            _disposed = true;
        }
    }
}
