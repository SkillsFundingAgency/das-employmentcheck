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
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;
        private ApiRetryOptions _settings;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();

            _configuration = _fixture.Build<DataCollectionsApiConfiguration>()
                .With(c => c.Url, "https://test.local")
                .Create();

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

            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions(It.IsAny<string>())).ReturnsAsync(_settings);

            
        }

        private void CreateDataCollectionsClient(HttpStatusCode statusCode)
        {
            _response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = statusCode
            };

            _httpMessageHandler = MessageHandler.SetupMessageHandlerMock(_response, _absoluteUrl);
            _httpClient = new HttpClient(_httpMessageHandler.Object);
            var hostingEnvironment = new Mock<IWebHostEnvironment>();
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(_httpClient);

            hostingEnvironment.Setup(x => x.EnvironmentName).Returns("Staging");

            _sut = new DataCollectionsApiClient(
                clientFactory.Object,
                _configuration,
                hostingEnvironment.Object,
                _tokenServiceMock.Object,
                Mock.Of<ILogger<DataCollectionsApiClient>>()
                );
        }

        [Test]
        public async Task Then_The_Endpoint_Is_Called()
        {
            //Arrange
            CreateDataCollectionsClient(HttpStatusCode.OK);

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
        public async Task Then_Using_Policy_The_Endpoint_Is_Called()
        {
            // Arrange
            CreateDataCollectionsClient(HttpStatusCode.OK);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            var ret = await retryPolicies.GetAll("Test");

            // Act
            var actual = await _sut.GetWithPolicy(ret, _request);

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
        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        public async Task Then_The_TokenService_Is_Called(HttpStatusCode httpStatusCode)
        {
            // Arrange
            CreateDataCollectionsClient(httpStatusCode);

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
        [TestCase(HttpStatusCode.OK)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        public async Task Then_Using_Policy_The_TokenService_Is_Called(HttpStatusCode httpStatusCode)
        {
            // Arrange
            CreateDataCollectionsClient(httpStatusCode);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            var ret = await retryPolicies.GetAll("Test");

            // Act
            var actual = await _sut.GetWithPolicy(ret, _request);

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
            // Arrange
            CreateDataCollectionsClient(HttpStatusCode.OK);

            // Act
            var actual = await _sut.Get(_request);

            // Assert
            actual.Should().BeEquivalentTo(_response);
        }

        [Test]
        public async Task Then_using_policy_the_response_is_returned()
        {
            // Arrange
            CreateDataCollectionsClient(HttpStatusCode.OK);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            var ret = await retryPolicies.GetAll("Test");

            // Act
            var actual = await _sut.GetWithPolicy(ret, _request);

            // Assert
            actual.Should().BeEquivalentTo(_response);
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Then_Error_Response_For_Transient_Errors_Is_Retried(HttpStatusCode httpStatusCode)
        {
            // Arrange
            CreateDataCollectionsClient(httpStatusCode);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            var ret = await retryPolicies.GetAll("Test");

            // Act
            var actual = await _sut.GetWithPolicy(ret, _request);

            // Assert
            _tokenServiceMock.Verify(_ => _.GetTokenAsync(
                $"https://login.microsoftonline.com/{_configuration.Tenant}",
                "client_credentials",
                _configuration.ClientSecret,
                _configuration.ClientId,
                _configuration.IdentifierUri
            ), Times.Exactly(3));
        }
    }
}
