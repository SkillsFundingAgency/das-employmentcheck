using AutoFixture;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.HashingService;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.EmployerAccountTests
{
    public class WhenGetEmployerPayeSchemes
    {
        private IEmployerAccountService _sut;
        private Fixture _fixture;
        private Mock<IAccountsResponseRepository> _repositoryMock;
        private Mock<IEmployerAccountApiClient<EmployerAccountApiConfiguration>> _apiClientMock;
        private Mock<IHashingService> _hashingServiceMock;
        private Data.Models.EmploymentCheck _employmentCheck;
        private string _hashedAccountId;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _apiClientMock = new Mock<IEmployerAccountApiClient<EmployerAccountApiConfiguration>>();
            _repositoryMock = new Mock<IAccountsResponseRepository>();
            _hashingServiceMock = new Mock<IHashingService>();
            _hashedAccountId = _fixture.Create<string>();
            _hashingServiceMock.Setup(_ => _.HashValue(_employmentCheck.AccountId)).Returns(_hashedAccountId);

            _sut = new EmployerAccountService(
                _hashingServiceMock.Object,
                _repositoryMock.Object,
                _apiClientMock.Object
            );
        }

        [Test]
        public async Task Then_AccountsApi_Is_Called()
        {
            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _apiClientMock.Verify(_ => _.Get(It.Is<GetAccountPayeSchemesRequest>(r =>
                r.GetUrl == $"api/accounts/{_hashedAccountId}/payeschemes")));
        }

        [Test]
        public async Task Then_Response_Is_Saved()
        {
            // Arrange
            var employerPayeSchemes = _fixture.CreateMany<ResourceViewModel>().ToList();
            var resourceList = new ResourceList(employerPayeSchemes);

            var expectedPayeShemes = string.Join(',', employerPayeSchemes.Select(x => x.Id.ToUpperInvariant()));

            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(resourceList)),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.Is<GetAccountPayeSchemesRequest>(
                    r => r.GetUrl == $"api/accounts/{_hashedAccountId}/payeschemes")))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<AccountsResponse>(
                        response => response.AccountId == _employmentCheck.AccountId
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == httpResponse.ToString()
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                                    && response.PayeSchemes == expectedPayeShemes
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Returned_Paye_Schemes_Are_Returned_To_Caller()
        {
            // Arrange
            var employerPayeSchemes = _fixture.CreateMany<ResourceViewModel>().ToList();
            var resourceList = new ResourceList(employerPayeSchemes);

            var payeSchemeList = employerPayeSchemes.Select(x => x.Id.ToUpperInvariant()).ToList();
            var expected = new EmployerPayeSchemes(_employmentCheck.AccountId, HttpStatusCode.OK, payeSchemeList);

            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(resourceList)),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.Is<GetAccountPayeSchemesRequest>(
                    r => r.GetUrl == $"api/accounts/{_hashedAccountId}/payeschemes")))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(expected);
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<AccountsResponse>(
                        response => response.PayeSchemes == null
                                    && response.AccountId == _employmentCheck.AccountId
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == ""
                                    && response.HttpStatusCode == (short)HttpStatusCode.InternalServerError
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unsuccessful_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = HttpStatusCode.BadRequest
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<AccountsResponse>(
                        response => response.PayeSchemes == null
                                    && response.AccountId == _employmentCheck.AccountId
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == httpResponse.ToString()
                                    && response.HttpStatusCode == (short)httpResponse.StatusCode
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Unsuccessful_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = HttpStatusCode.BadRequest
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ThrowsAsync(exception);

            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _repositoryMock.Verify(repository => repository.InsertOrUpdate(It.Is<AccountsResponse>(
                        response => response.PayeSchemes == null
                                    && response.AccountId == _employmentCheck.AccountId
                                    && response.ApprenticeEmploymentCheckId == _employmentCheck.Id
                                    && response.CorrelationId == _employmentCheck.CorrelationId
                                    && response.HttpResponse == exception.Message
                                    && response.HttpStatusCode == (short)HttpStatusCode.InternalServerError
                    )
                )
                , Times.Once());
        }

        [Test]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ThrowsAsync(_fixture.Create<Exception>());

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_Null_As_EmployerPayeSchemes_Is_Returned_In_Case_Of_Empty_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };

            _apiClientMock.Setup(_ => _.Get(It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }
    }
}