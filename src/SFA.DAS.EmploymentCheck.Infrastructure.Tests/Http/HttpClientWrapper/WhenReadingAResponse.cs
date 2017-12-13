using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Domain;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Tests.Http.HttpClientWrapper
{
    [TestFixture]
    public class WhenReadingAResponse : HttpWrapperTestBase
    {
        private string _jsonGetResponse =
                "{\"PageNumber\":1,\"TotalNumberOfPages\":1,\"Items\":[{\"Id\":1,\"EmployerReferenceNumber\":123456},{\"Id\":2,\"EmployerReferenceNumber\":123456},{\"Id\":3,\"EmployerReferenceNumber\":123456},{\"Id\":4,\"EmployerReferenceNumber\":123456},{\"Id\":5,\"EmployerReferenceNumber\":123456},{\"Id\":6,\"EmployerReferenceNumber\":123456}]}";

        [Test]
        public void ThenItIsCheckedToSeeIfItWasSuccessful()
        {
            Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await _httpClientWrapper.ReadResponse<string>(new HttpResponseMessage(HttpStatusCode.BadRequest));
            });
        }

        [Test]
        public async Task ThenItDeserialisesTheResponse()
        {
            var msg = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(_jsonGetResponse, Encoding.UTF8, "application/json")
            };

            var response = await _httpClientWrapper.ReadResponse<PageOfResults<ApiItems>>(msg);

            Assert.AreEqual(6, response.Items.Length);
        }

        internal class ApiItems
        {
            public long Id { get; set; }

            public string EmployerReferenceNumber { get; set; }
        }
    }
}
