using AutoFixture;
using Boxed.AspNetCore;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.Learner;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.DataCollectionsApiClientTests
{
    public class WhenCallingGetAndReceiveUnsuccessfulResponse
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
                StatusCode = HttpStatusCode.BadRequest
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
        public void Then_the_response_is_returned()
        {
            // Act
            Func<Task> action = async () => { await _sut.Get(_request); };

            // Asserts
            action.Should().ThrowAsync<HttpException>();
        }
    }
}
