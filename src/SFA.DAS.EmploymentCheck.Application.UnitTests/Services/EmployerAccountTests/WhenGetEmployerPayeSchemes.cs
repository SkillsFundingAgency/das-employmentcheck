﻿using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Polly.Wrap;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.EmploymentCheck.Application.Services;
using SFA.DAS.EmploymentCheck.Application.Services.EmployerAccount;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
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
        private Data.Models.EmploymentCheck _employmentCheck;
        private Mock<IApiOptionsRepository> _apiOptionsRepositoryMock;
        private ApiRetryOptions _settings;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _employmentCheck = _fixture.Build<Data.Models.EmploymentCheck>().Create();

            _apiClientMock = new Mock<IEmployerAccountApiClient<EmployerAccountApiConfiguration>>();
            _repositoryMock = new Mock<IAccountsResponseRepository>();
            
            _apiOptionsRepositoryMock = new Mock<IApiOptionsRepository>();

            _settings = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1
            };

            _apiOptionsRepositoryMock.Setup(r => r.GetOptions()).Returns(_settings);

            var retryPolicies = new ApiRetryPolicies(
                Mock.Of<ILogger<ApiRetryPolicies>>(),
                _apiOptionsRepositoryMock.Object);

            _sut = new EmployerAccountService(
                _repositoryMock.Object,
                _apiClientMock.Object,
                retryPolicies
            );
        }

        [Test]
        public async Task Then_AccountsApi_Is_Called()
        {
            // Act
            await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            _apiClientMock.Verify(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetAccountPayeSchemesRequest>(r =>
                r.GetUrl == $"api/accounts/{_employmentCheck.AccountId}/payeschemes")));
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

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetAccountPayeSchemesRequest>(
                    r => r.GetUrl == $"api/accounts/{_employmentCheck.AccountId}/payeschemes")))
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

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.Is<GetAccountPayeSchemesRequest>(
                    r => r.GetUrl == $"api/accounts/{_employmentCheck.AccountId}/payeschemes")))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeEquivalentTo(expected);
            result.HttpStatusCode = expected.HttpStatusCode;
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Null_Response()
        {
            // Arrange
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
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
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync((HttpResponseMessage)null);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unsuccessful_Response(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(_fixture.Create<string>()),
                StatusCode = httpStatusCode
            };

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
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
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.InternalServerError)]
        public async Task Then_Null_Is_Returned_To_Caller_In_Case_Of_Unsuccessful_Response(HttpStatusCode httpStatusCode)
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = httpStatusCode
            };

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(httpResponse.StatusCode);
            result.PayeSchemes.Count().Should().Be(0);
        }

        [Test]
        public async Task Then_Error_Response_Is_Saved_In_Case_Of_Unexpected_Exception()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
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
            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
                .ThrowsAsync(_fixture.Create<Exception>());

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task Then_EmployerPayeSchemes_Is_Returned_In_Case_Of_Empty_Response()
        {
            // Arrange
            var httpResponse = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.OK
            };
            var employerPayeSchemes = _fixture.Build<EmployerPayeSchemes>()
                .With(x => x.PayeSchemes, () => null)
                .With(x => x.HttpStatusCode, httpResponse.StatusCode)
                .Create();

            _apiClientMock.Setup(_ => _.GetWithPolicy(It.IsAny<AsyncPolicyWrap>(), It.IsAny<GetAccountPayeSchemesRequest>()))
                .ReturnsAsync(httpResponse);

            // Act
            var result = await _sut.GetEmployerPayeSchemes(_employmentCheck);

            // Assert
            result.Should().NotBeNull();
            result.HttpStatusCode.Should().Be(employerPayeSchemes.HttpStatusCode);
            result.PayeSchemes.Should().NotBeNull();
            result.PayeSchemes.Count().Should().Be(0);
        }
    }
}