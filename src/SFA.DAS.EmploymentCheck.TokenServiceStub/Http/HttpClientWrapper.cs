using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.TokenServiceStub.Http
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public HttpClientWrapper()
        {
            AcceptHeaders = new List<MediaTypeWithQualityHeaderValue>
            {
                new MediaTypeWithQualityHeaderValue("application/vnd.hmrc.1.0+json")
            };
        }

        public List<MediaTypeWithQualityHeaderValue> AcceptHeaders { get; set; }

        public async Task<T> Post<T>(string url, object content)
        {
            using (var client = CreateClient())
            {
                var jsonContent = JsonConvert.SerializeObject(content);
                var response = await client.PostAsync(url, new StringContent(jsonContent, Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Http status code {response.StatusCode} indicates failure. (Status description: {response.ReasonPhrase}, body: {responseBody})");
                }
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
        }


        private HttpClient CreateClient()
        {
            var client = new HttpClient();

            if (AcceptHeaders.Count > 0)
            {
                client.DefaultRequestHeaders.Accept.Clear();
                foreach (var accept in AcceptHeaders)
                {
                    client.DefaultRequestHeaders.Accept.Add(accept);
                }
            }

            return client;
        }
    }
}