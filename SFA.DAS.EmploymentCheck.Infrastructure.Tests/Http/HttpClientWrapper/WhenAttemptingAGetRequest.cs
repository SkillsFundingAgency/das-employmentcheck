using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Domain;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Tests.Http.HttpClientWrapper
{
    [TestFixture]
    public class WhenAttemptingAGetRequest : HttpWrapperTestBase
    {
        [Test]
        public async Task ThenTheGivenUrlIsCalled()
        {
            SetupMessageHandler(HttpStatusCode.OK, (r, c) =>
            {
                Assert.AreEqual(HttpMethod.Get, r.Method);
                Assert.AreEqual(_requestUri, r.RequestUri.ToString());
            });
           
            await _httpClientWrapper.GetAsync(_requestUri.ToUri(), Constants.ContentTypeValue);
        }
    }
}
