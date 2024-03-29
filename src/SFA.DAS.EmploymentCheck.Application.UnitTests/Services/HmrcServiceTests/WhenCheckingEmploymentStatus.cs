﻿using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Data;
using SFA.DAS.EmploymentCheck.Data.Models;
using SFA.DAS.EmploymentCheck.Data.Repositories.Interfaces;
using SFA.DAS.EmploymentCheck.Domain.Enums;
using SFA.DAS.EmploymentCheck.Infrastructure.Configuration;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Application.UnitTests.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private IHmrcService _sut;

        private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyServiceMock;
        private Mock<IHmrcApiOptionsRepository> _rateLimiterRepositoryMock;
        private Mock<ITokenServiceApiClient> _tokenServiceMock;
        private PrivilegedAccessToken _token;
        private EmploymentCheckCacheRequest _request;
        private Fixture _fixture;
        private HmrcApiRateLimiterOptions _hmrcApiRateLimiterOptions;
        private Mock<IOptions<ApiRetryOptions>> _apiRetryOptionsMock;
        private ApiRetryOptions _apiRetryOptions;
        private ApiRetryDelaySettings _apiRetryDelaySettings;
        private Mock<IEmploymentCheckService> _employmentCheckServiceMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apprenticeshipLevyServiceMock = new Mock<IApprenticeshipLevyApiClient>();
            _tokenServiceMock = new Mock<ITokenServiceApiClient>();
            _rateLimiterRepositoryMock = new Mock<IHmrcApiOptionsRepository>();
            _employmentCheckServiceMock = new Mock<IEmploymentCheckService>();
            _apiRetryOptionsMock = new Mock<IOptions<ApiRetryOptions>>();

            _token = new PrivilegedAccessToken { AccessCode = _fixture.Create<string>(), ExpiryTime = DateTime.Today.AddDays(7) };

            _tokenServiceMock.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _request = _fixture.Create<EmploymentCheckCacheRequest>();
            _request.MinDate = _fixture.Create<DateTime>();
            _request.MaxDate = _request.MinDate.AddMonths(-6);

            _hmrcApiRateLimiterOptions = new HmrcApiRateLimiterOptions
            {
                MinimumReduceDelayIntervalInMinutes = 0,
                TokenFailureRetryDelayInMs = 0
            };

            _apiRetryOptions = new ApiRetryOptions
            {
                TooManyRequestsRetryCount = 10,
                TransientErrorRetryCount = 2,
                TransientErrorDelayInMs = 1,
                TokenRetrievalRetryCount = 2
            };
            _apiRetryOptionsMock.Setup(x => x.Value).Returns(_apiRetryOptions);

            _apiRetryDelaySettings = new ApiRetryDelaySettings
            {
                DelayInMs = 0
            };

            _rateLimiterRepositoryMock.Setup(r => r.GetHmrcRateLimiterOptions()).Returns(_hmrcApiRateLimiterOptions);

            var retryPolicies = new HmrcApiRetryPolicies(
                Mock.Of<ILogger<HmrcApiRetryPolicies>>(),
                _rateLimiterRepositoryMock.Object,
                _apiRetryOptionsMock.Object,
                _apiRetryDelaySettings);

            _sut = new HmrcService(
                new HmrcTokenStore(_tokenServiceMock.Object, retryPolicies, Mock.Of<ILogger<HmrcTokenStore>>()),
                _apprenticeshipLevyServiceMock.Object,
                Mock.Of<ILogger<HmrcService>>(),
                _employmentCheckServiceMock.Object,
                retryPolicies);
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called()
        {
            // Arrange
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Call_Is_Retried_On_Error()
        {
            // Arrange
            var httpException = new ApiHttpException((int)HttpStatusCode.InternalServerError, "error", "/get-token",
                string.Empty, new Exception("failed"));
            _tokenServiceMock.Setup(x => x.GetPrivilegedAccessTokenAsync()).ThrowsAsync(httpException);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly((_apiRetryOptions.TokenRetrievalRetryCount + 1) * _apiRetryOptions.TransientErrorRetryCount));
        }

        [Test]
        public async Task Then_Cached_AccessToken_Is_Reused()
        {
            // Arrange
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            var token = new PrivilegedAccessToken
            {
                AccessCode = _fixture
                .Create<string>(),
                ExpiryTime = DateTime.Now.AddHours(1)
            };

            _tokenServiceMock.Setup(ts => ts.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(token);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called_Again_When_Token_Expires()
        {
            // Arrange
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            var expiredToken = new PrivilegedAccessToken
            {
                AccessCode = _fixture
                .Create<string>(),
                ExpiryTime = DateTime.UtcNow.AddMilliseconds(100)
            };

            _tokenServiceMock.Setup(ts => ts.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(expiredToken);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await Task.Delay(TimeSpan.FromSeconds(1));
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(2));
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called_When_UnauthorizedAccess_ApiException_Is_Returned()
        {
            // Arrange
            const short code = (short)HttpStatusCode.Unauthorized;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(_apiRetryOptions.TransientErrorRetryCount + 1));
        }

        [Test]
        public async Task Then_a_positive_response_is_saved_as_complete()
        {
            // Arrange
            var response = _fixture.Create<EmploymentStatus>();
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ReturnsAsync(response);

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x => x.Id == _request.Id),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == response.Employed
                        && x.FoundOnPaye == response.Empref
                        && x.ProcessingComplete
                        && x.Count == 1
                        && x.HttpResponse == "OK"
                        && x.HttpStatusCode == 200
                )
            ), Times.Once);
        }

        [Test]
        public async Task Then_a_request_is_saved_as_completed_when_positive_response_is_returned()
        {
            // Arrange
            var response = _fixture.Create<EmploymentStatus>();
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(response);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == response.Employed
                        && x.MinDate == _request.MinDate
                        && x.MaxDate == _request.MaxDate
                        && x.RequestCompletionStatus == (short)ProcessingCompletionStatus.Completed),
                    It.IsAny<EmploymentCheckCacheResponse>()
            ), Times.Once);
        }

        [Test]
        public async Task Then_NotFound_response_is_saved_as_complete_without_retrying()
        {
            // Arrange
            const short code = (short)HttpStatusCode.NotFound;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Nino == _request.Nino
                        && x.PayeScheme == _request.PayeScheme
                        && x.MinDate == _request.MinDate
                        && x.Employed == _request.Employed
                        && x.RequestCompletionStatus == _request.RequestCompletionStatus
                ),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )
            ), Times.Once);

            _apprenticeshipLevyServiceMock.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(1));
        }

        [Test]
        public async Task Then_TooManyRequests_response_is_saved_as_complete()
        {
            // Arrange
            const short code = (short)HttpStatusCode.TooManyRequests;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Nino == _request.Nino
                        && x.PayeScheme == _request.PayeScheme
                        && x.MinDate == _request.MinDate
                        && x.Employed == _request.Employed
                        && x.RequestCompletionStatus == _request.RequestCompletionStatus
                ),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete == true
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                    )
                ), Times.Once);
        }

        [Test]
        public async Task Then_TooManyRequests_response_is_retried()
        {
            // Arrange
            const short code = (short)HttpStatusCode.TooManyRequests;

            // Arrange
            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
            _apprenticeshipLevyServiceMock.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(_apiRetryOptions.TooManyRequestsRetryCount + 1));
        }


        [Test]
        public async Task Then_TooManyRequests_Adjusts_Rate_Limiter_Settings()
        {
            // Arrange
            const short code = (short)HttpStatusCode.TooManyRequests;

            // Arrange
            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _rateLimiterRepositoryMock.Verify(x => x.GetHmrcRateLimiterOptions(), Times.Exactly(_apiRetryOptions.TooManyRequestsRetryCount + 2));
            _rateLimiterRepositoryMock.Verify(x => x.IncreaseDelaySetting(It.IsAny<HmrcApiRateLimiterOptions>()), Times.Exactly(_apiRetryOptions.TooManyRequestsRetryCount));
        }

        [TestCase((short)HttpStatusCode.Unauthorized, TestName = "Then_Unauthorized_response_is_saved_as_complete")]
        [TestCase((short)HttpStatusCode.BadGateway, TestName = "Then_BadGateway_response_is_saved_as_complete")]
        [TestCase((short)HttpStatusCode.RequestTimeout, TestName = "Then_RequestTimeout_response_is_saved_as_complete")]
        [TestCase((short)HttpStatusCode.InternalServerError, TestName = "Then_InternalServerError_response_is_saved_as_complete")]
        [TestCase((short)HttpStatusCode.ServiceUnavailable, TestName = "Then_ServiceUnavailable_response_is_saved_as_complete")]
        public async Task Then_Transient_Error_responses_are_saved_as_complete(short httpStatusCode)
        {
            // Arrange
            var exception = new ApiHttpException(
                httpStatusCode,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Nino == _request.Nino
                        && x.PayeScheme == _request.PayeScheme
                        && x.MinDate == _request.MinDate
                        && x.Employed == _request.Employed
                        && x.RequestCompletionStatus == _request.RequestCompletionStatus
                ),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete == true
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == httpStatusCode
                )
            ), Times.Once);
        }

        [TestCase((short)HttpStatusCode.Unauthorized, TestName = "Then_Unauthorized_response_is_retried_2_times")]
        [TestCase((short)HttpStatusCode.BadGateway, TestName = "Then_BadGateway_response_is_retried_2_times")]
        [TestCase((short)HttpStatusCode.RequestTimeout, TestName = "Then_RequestTimeout_response_is_retried_2_times")]
        [TestCase((short)HttpStatusCode.InternalServerError, TestName = "Then_InternalServerError_response_is_retried_2_times")]
        [TestCase((short)HttpStatusCode.ServiceUnavailable, TestName = "Then_ServiceUnavailable_response_is_retried_2_times")]
        public async Task Then_Transient_Error_responses_are_retried_2_times(short httpStatusCode)
        {
            // Arrange
            var exception = new ApiHttpException(
                httpStatusCode,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(3));
            _apprenticeshipLevyServiceMock.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(3));
        }

        [TestCase((short)HttpStatusCode.BadRequest, TestName = "Then_BadRequest_response_is_saved_as_completed_without_retrying")]
        [TestCase((short)HttpStatusCode.Forbidden, TestName = "Then_Forbidden_response_is_saved_as_completed_without_retrying")]
        [TestCase((short)HttpStatusCode.NotFound, TestName = "Then_NotFound_response_is_saved_as_completed_without_retrying")]
        public async Task Then_expected_error_responses_are_saved_as_completed_without_retrying(short httpStatusCode)
        {
            // Arrange
            var exception = new ApiHttpException(
                httpStatusCode,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Nino == _request.Nino
                        && x.PayeScheme == _request.PayeScheme
                        && x.MinDate == _request.MinDate
                        && x.Employed == _request.Employed
                        && x.RequestCompletionStatus == _request.RequestCompletionStatus
                ),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                    x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                    && x.EmploymentCheckCacheRequestId == _request.Id
                    && x.CorrelationId == _request.CorrelationId
                    && x.Employed == null
                    && x.FoundOnPaye == null
                    && x.ProcessingComplete
                    && x.Count == 1
                    && x.HttpResponse == exception.ResourceUri
                    && x.HttpStatusCode == httpStatusCode
                 )
            ), Times.Once);

            _apprenticeshipLevyServiceMock.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(1));
        }

        [Test]
        public async Task Then_generic_Exception_is_saved_as_complete()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut
                .IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _employmentCheckServiceMock.Verify(r => r.StoreCompletedCheck(
                It.Is<EmploymentCheckCacheRequest>(
                    x =>
                        x.Id == _request.Id
                        && x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.CorrelationId == _request.CorrelationId
                        && x.Nino == _request.Nino
                        && x.PayeScheme == _request.PayeScheme
                        && x.MinDate == _request.MinDate
                        && x.Employed == _request.Employed
                        && x.RequestCompletionStatus == _request.RequestCompletionStatus
                ),
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                    x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                    && x.EmploymentCheckCacheRequestId == _request.Id
                    && x.CorrelationId == _request.CorrelationId
                    && x.Employed == null
                    && x.FoundOnPaye == null
                    && x.ProcessingComplete == true
                    && x.Count == 1
                    && x.HttpResponse == exception.Message
                    && x.HttpStatusCode == 500
                )
            ), Times.Once);
        }

        [Test]
        public async Task Then_UnauthorizedAccessException_is_retried_twice()
        {
            // Arrange
            var exception = _fixture.Create<UnauthorizedAccessException>();

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(_apiRetryOptions.TransientErrorRetryCount + 1));
            _apprenticeshipLevyServiceMock.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(_apiRetryOptions.TransientErrorRetryCount + 1));
        }

        [Test]
        public async Task Then_a_new_token_is_retrieved_for_each_retry_of_any_ApiException()
        {
            // Arrange
            var exception = new ApiHttpException(
                (int)HttpStatusCode.NoContent,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenServiceMock.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(_apiRetryOptions.TransientErrorRetryCount + 1));
        }

        [Test]
        public async Task Then_retry_delay_time_is_increased_When_TooManyRequests()
        {
            // Arrange
            const short code = (short)HttpStatusCode.TooManyRequests;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _rateLimiterRepositoryMock.Verify(r => r.IncreaseDelaySetting(It.IsAny<HmrcApiRateLimiterOptions>()),
                Times.Exactly(_apiRetryOptions.TooManyRequestsRetryCount));
        }

        [Test]
        public async Task Then_retry_delay_time_is_decreased_When_successful_response()
        {
            // Arrange
            _apprenticeshipLevyServiceMock.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _rateLimiterRepositoryMock.Verify(r => r.ReduceDelaySetting(It.IsAny<HmrcApiRateLimiterOptions>()),
                Times.Once);
        }
    }
}