using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.EmployerAccountApiClient
{
    public class WhenCallingGet
    {
        private Application.Services.EmployerAccount.EmployerAccountApiClient _sut;
        private Fixture _fixture;
        private Mock<IAzureClientCredentialHelper> _tokenServiceMock;
        private GetNationalInsuranceNumberRequest _request;
        private Mock<HttpMessageHandler> _httpMessageHandler;
        private EmployerAccountApiConfiguration _configuration;
        private string _token;
        private HttpClient _httpClient;
        private string _absoluteUrl;
        private HttpResponseMessage _response;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _configuration = _fixture.Build<EmployerAccountApiConfiguration>()
                .With(c => c.Url, "https://test.local")
                .Create();

            _response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };

            _token = _fixture.Create<string>();

            _tokenServiceMock = new Mock<IAzureClientCredentialHelper>();
            _tokenServiceMock.Setup(ts => ts.GetAccessTokenAsync(_configuration.Identifier))
                .ReturnsAsync(_token);

            _request = _fixture.Create<GetNationalInsuranceNumberRequest>();
            _absoluteUrl = _configuration.Url + _request.GetUrl;

            _httpMessageHandler = MessageHandler.SetupMessageHandlerMock(_response, _absoluteUrl);
            _httpClient = new HttpClient(_httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");

            _sut = new Application.Services.EmployerAccount.EmployerAccountApiClient(
                clientFactory.Object,
                _configuration,
                _tokenServiceMock.Object, hostingEnvironment.Object);
        }

        [Test]
        public async Task Then_The_Endpoint_Is_Called()
        {
            // Act
            await _sut.Get(_request);

            // Assert
            _httpMessageHandler.Protected()
                .Verify<Task<HttpResponseMessage>>(
                    "SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(
                        c =>
                            c.Method.Equals(HttpMethod.Get)
                            && c.RequestUri.AbsoluteUri.Equals(_absoluteUrl)
                            && c.Headers.Authorization.Scheme.Equals("Bearer")
                            && c.Headers.Authorization.Parameter.Equals(_token)
                    ),
                    ItExpr.IsAny<CancellationToken>()
                );
        }

        [Test]
        public async Task Then_The_TokenService_Is_Called()
        {
            // Act
            await _sut.Get(_request);

            // Assert
            _tokenServiceMock.Verify(_ => _.GetAccessTokenAsync(_configuration.Identifier), Times.Once);
        }

        [Test]
        public async Task Then_the_response_is_returned()
        {
            // Act
            var actual = await _sut.Get(_request);

            // Assert
            actual.Should().BeEquivalentTo(_response);
        }
    }
}
