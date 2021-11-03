using System;
using System.Net.Http;
using Microsoft.AspNetCore.Hosting;
using Moq;
using SFA.DAS.Api.Common.Interfaces;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmployerAccount;
using SFA.DAS.EmploymentCheck.Functions.Configuration;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Clients.EmployerAccountApiClientTests
{
    public class WhenCallingGet
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactory;
        private readonly Mock<HttpClient> _httpClient;
        private readonly Mock<IWebHostEnvironment> _hostingEnvironment;
        private readonly Mock<EmployerAccountApiConfiguration> _configuration;
        private readonly Mock<IAzureClientCredentialHelper> _azureClientCredentialHelper;
        private readonly Mock<ILoggerAdapter<IEmployerAccountApiClient>> _logger;

        public WhenCallingGet()
        {
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _httpClient = new Mock<HttpClient>();
            //_httpClientFactory.Setup(x => x.CreateClient()).Returns(_httpClient.Object);
            _hostingEnvironment = new Mock<IWebHostEnvironment>();
            _configuration = new Mock<EmployerAccountApiConfiguration>();
            _azureClientCredentialHelper = new Mock<IAzureClientCredentialHelper>();
            _logger = new Mock<ILoggerAdapter<IEmployerAccountApiClient>>();
        }

        [Fact]
        public async void And_the_Client_Throws_An_Exception_Then_It_Is_Logged()
        {
            //Arrange

            var exception = new Exception("exception");
            _configuration.Object.Url = "api/accounts/internal/1";
            //_httpClient.Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>())).Throws(exception);

            var sut = new EmployerAccountApiClient(_httpClientFactory.Object, _configuration.Object,
                _hostingEnvironment.Object, _azureClientCredentialHelper.Object, _logger.Object);

            //Act

            await sut.Get<Type>("");

            //Assert

            _logger.Verify(x =>
                x.LogInformation(
                    $"\n\nEmployerAccountApiClient.Get(): Exception caught - {exception.Message}. {exception.StackTrace}"));
        }
    }
}