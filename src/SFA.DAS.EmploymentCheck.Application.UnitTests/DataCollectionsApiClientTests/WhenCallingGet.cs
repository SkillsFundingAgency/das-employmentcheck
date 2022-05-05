using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.DataCollectionsApiClientTests
{
    public class WhenCallingGet
    {
        private DataCollectionsApiClient _sut;
        private Fixture _fixture;
        private Mock<IDcTokenService> _tokenServiceMock;
        private GetNationalInsuranceNumberRequest _request;
        private Mock<HttpMessageHandler> _httpMessageHandler;
        private DataCollectionsApiConfiguration _configuration;
        private AuthResult _token;
        private HttpClient _httpClient;
        private string _absoluteUrl;
        private HttpResponseMessage _response;
        private ApiRetryOptions _settings;
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _configuration = _fixture.Build<DataCollectionsApiConfiguration>()
                .With(c => c.Url, "https://test.local")
                .Create();

            _response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };

            _token = _fixture.Create<AuthResult>();

            _tokenServiceMock = new Mock<IDcTokenService>();
            _tokenServiceMock.Setup(ts => ts.GetTokenAsync(
                $"https://login.microsoftonline.com/{_configuration.Tenant}",
                "client_credentials",
                _configuration.ClientSecret,
                _configuration.ClientId,
                _configuration.IdentifierUri
                )).ReturnsAsync(_token);

            _request = _fixture.Create<GetNationalInsuranceNumberRequest>();
            _absoluteUrl = _configuration.Url + _request.GetUrl;

            _httpMessageHandler = MessageHandler.SetupMessageHandlerMock(_response, _absoluteUrl);
            _httpClient = new HttpClient(_httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");

            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions()).ReturnsAsync(_settings);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            _sut = new DataCollectionsApiClient(
                clientFactory.Object,
                _configuration,
                hostingEnvironment.Object,
                _tokenServiceMock.Object,
                retryPolicies,
                Mock.Of<ILogger<DataCollectionsApiClient>>()
                );
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
                            && c.Headers.Authorization.Parameter.Equals(_token.AccessToken)
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
            _tokenServiceMock.Verify(_ => _.GetTokenAsync(
                $"https://login.microsoftonline.com/{_configuration.Tenant}",
                "client_credentials",
                _configuration.ClientSecret,
                _configuration.ClientId,
                _configuration.IdentifierUri
            ), Times.Once);
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
