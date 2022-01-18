using AutoFixture;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Repositories;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private Mock<ILogger<HmrcService>> _logger;
        private Mock<IEmploymentCheckCacheResponseRepository> _repository;
        private Mock<ITokenServiceApiClient> _tokenService;
        private PrivilegedAccessToken _token;
        private EmploymentCheckCacheRequest _request;
        private Fixture _fixture;
        private HmrcService _sut;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _logger = new Mock<ILogger<HmrcService>>();
            _tokenService = new Mock<ITokenServiceApiClient>();
            _repository = new Mock<IEmploymentCheckCacheResponseRepository>();

            _token = new PrivilegedAccessToken {AccessCode = _fixture.Create<string>(), ExpiryTime = DateTime.Today.AddDays(7)};

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _request = _fixture.Create<EmploymentCheckCacheRequest>();
            _request.MinDate = _fixture.Create<DateTime>();
            _request.MaxDate = _request.MinDate.AddMonths(-6);

            _sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object, _repository.Object);
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
        public async Task Then_The_TokenServiceApiClient_Is_CalledWhenExpired()
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
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(2));
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
                        //&& x.FoundOnPaye == _request.PayeScheme
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
        public async Task Then_NotFound_response_is_saved_as_complete()
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
                        //&& x.FoundOnPaye == string.Empty
                        && x.ProcessingComplete
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )

            ), Times.Once);
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
                        //&& x.FoundOnPaye == string.Empty
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )

            ), Times.Once);
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
                        //&& x.FoundOnPaye == string.Empty
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )

            ), Times.Once);
        }

        [Test]
        public async Task Then_BadRequest_response_is_saved_as_complete()
        {
            // Arrange
            const short code = (short)HttpStatusCode.BadRequest;

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
                        //&& x.FoundOnPaye == string.Empty
                        && x.ProcessingComplete
                        && x.Count == 1
                        && x.HttpResponse == exception.ResourceUri
                        && x.HttpStatusCode == code
                )

            ), Times.Once);
        }

        [Test]
        public async Task Then_Error_response_is_saved_as_incomplete()
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
                        //&& x.FoundOnPaye == string.Empty
                        && x.ProcessingComplete == false
                        && x.Count == 1
                        && x.HttpResponse == exception.Message
                        && x.HttpStatusCode == 500
                )

            ), Times.Once);
        }
    }
}