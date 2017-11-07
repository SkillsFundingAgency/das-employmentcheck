using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace SFA.DAS.EmploymentCheck.Infrastructure.Tests.Http.HttpClientWrapper
{
    public abstract class HttpWrapperTestBase
    {
        protected Infrastructure.Http.HttpClientWrapper _httpClientWrapper;
        protected Mock<HttpMessageHandler> _messageHandler;
        protected string _requestUri = "http://www.site.com/api/path";

        [SetUp]
        public void Arrange()
        {
            _messageHandler = new Mock<HttpMessageHandler>();
            _httpClientWrapper = new Infrastructure.Http.HttpClientWrapper(_messageHandler.Object);
        }

        protected void SetupMessageHandler(HttpStatusCode statusCode, Action<HttpRequestMessage, CancellationToken> callBack)
        {
            _messageHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage()
                {
                    StatusCode = statusCode
                })).Callback<HttpRequestMessage, CancellationToken>(callBack);
        }
    }
}
