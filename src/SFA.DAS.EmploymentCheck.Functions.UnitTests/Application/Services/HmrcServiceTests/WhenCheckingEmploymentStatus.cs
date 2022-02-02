using System;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private Mock<IEmploymentCheckCacheResponseRepository> _repository;
        private Mock<ITokenServiceApiClient> _tokenService;
        private PrivilegedAccessToken _token;
        private EmploymentCheckCacheRequest _request;
        private Fixture _fixture;
        private IHmrcService _sut;
        private HmrcApiRetryPolicySettings _settings;
        // private Mock<IOptions<HmrcApiRetryPolicySettings>> _settingsMock;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _tokenService = new Mock<ITokenServiceApiClient>();
            _repository = new Mock<IEmploymentCheckCacheResponseRepository>();

            _token = new PrivilegedAccessToken {AccessCode = _fixture.Create<string>(), ExpiryTime = DateTime.Today.AddDays(7)};

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _request = _fixture.Create<EmploymentCheckCacheRequest>();
            _request.MinDate = _fixture.Create<DateTime>();
            _request.MaxDate = _request.MinDate.AddMonths(-6);

            _settings = new HmrcApiRetryPolicySettings
            {
                TransientErrorDelayInMs = 1,
                TransientErrorRetryCount = 2,
                TooManyRequestsRetryCount = 10
            };

            var retryPolicies = new HmrcApiRetryPolicies(
                Mock.Of<ILogger<HmrcApiRetryPolicies>>(),
                new OptionsWrapper<HmrcApiRetryPolicySettings>(_settings));

            _sut = new HmrcService(
                _tokenService.Object, 
                _apprenticeshipLevyService.Object, 
                Mock.Of<ILogger<HmrcService>>(), 
                _repository.Object,
                retryPolicies);
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called()
        {
            // Arrange
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_Cached_AccessToken_Is_Reused()
        {
            // Arrange
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            var token = new PrivilegedAccessToken
            {
                AccessCode = _fixture.Create<string>(),
                ExpiryTime = DateTime.Now.AddHours(1)
            };

            _tokenService.Setup(ts => ts.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(token);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called_Again_When_Token_Expires()
        {
            // Arrange
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ReturnsAsync(_fixture.Create<EmploymentStatus>());

            var expiredToken = new PrivilegedAccessToken
            {
                AccessCode = _fixture.Create<string>(),
                ExpiryTime = DateTime.Now.AddSeconds(-1)
            };

            _tokenService.Setup(ts => ts.GetPrivilegedAccessTokenAsync())
                .ReturnsAsync(expiredToken);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(2));
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

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(_settings.TransientErrorRetryCount + 1));
        }

        [Test]
        public async Task Then_a_positive_response_is_saved_as_complete()
        {
            // Arrange
            var response = _fixture.Create<EmploymentStatus>();
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ReturnsAsync(response);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
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
        public async Task Then_a_null_response_is_handled()
        {
            // Arrange
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ReturnsAsync((EmploymentStatus)null);

            // Act
            var result = await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
                It.IsAny<EmploymentCheckCacheResponse>()
            ), Times.Once);

            result.Employed.Should().BeNull();
            result.RequestCompletionStatus.Should().Be(500);
        }

        [Test]
        public async Task Then_NotFound_response_is_saved_as_complete_without_retrying()
        {
            // Arrange
            const short code = (short) HttpStatusCode.NotFound;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
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

            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(1));
        }

        [Test]
        public async Task Then_TooManyRequests_response_is_saved_as_incomplete()
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

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )

            ), Times.Once);
        }

        [Test]
        public async Task Then_TooManyRequests_response_is_retried_10_times()
        {
            // Arrange
            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(new UnauthorizedAccessException());

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(_settings.TooManyRequestsRetryCount + 1));
        }

        [Test]
        public async Task Then_RequestTimeout_response_is_saved_as_incomplete()
        {
            // Arrange
            const short code = (short)HttpStatusCode.RequestTimeout;

            var exception = new ApiHttpException(
                code,
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<string>(),
                _fixture.Create<Exception>()
            );

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
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

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                    _token.AccessCode,
                    _request.PayeScheme,
                    _request.Nino,
                    _request.MinDate,
                    _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(3));
            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(3));
        }

        [TestCase((short)HttpStatusCode.BadRequest, TestName= "Then_BadRequest_response_is_saved_as_completed_without_retrying")]
        [TestCase((short)HttpStatusCode.Forbidden, TestName= "Then_Forbidden_response_is_saved_as_completed_without_retrying")]
        [TestCase((short)HttpStatusCode.NotFound, TestName= "Then_NotFound_response_is_saved_as_completed_without_retrying")]
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

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
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

            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate), Times.Exactly(1));
        }

        [Test]
        public async Task Then_AnyOtherException_is_saved_as_incomplete()
        {
            // Arrange
            var exception = _fixture.Create<Exception>();

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(
                _token.AccessCode,
                _request.PayeScheme,
                _request.Nino,
                _request.MinDate,
                _request.MaxDate))
                .ThrowsAsync(exception);

            // Act
            await _sut.IsNationalInsuranceNumberRelatedToPayeScheme(_request);

            // Assert
            _repository.Verify(r => r.Save(
                It.Is<EmploymentCheckCacheResponse>(
                    x =>
                        x.ApprenticeEmploymentCheckId == _request.ApprenticeEmploymentCheckId
                        && x.EmploymentCheckCacheRequestId == _request.Id
                        && x.CorrelationId == _request.CorrelationId
                        && x.Employed == null
                        && x.FoundOnPaye == null
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.Message
                        && x.HttpStatusCode == 500
                )

            ), Times.Once);
        }
    }
}