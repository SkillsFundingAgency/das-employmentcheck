using AutoFixture;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private readonly Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private readonly Mock<ILogger<HmrcService>> _logger;
        private readonly Mock<ITokenServiceApiClient> _tokenService;
        private readonly PrivilegedAccessToken _token;
        private readonly ApprenticeEmploymentCheckModel _apprentice;
        private readonly Fixture _fixture;
     
        public WhenCheckingEmploymentStatus()
        {
            _fixture = new Fixture();
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _logger = new Mock<ILogger<HmrcService>>();
            _tokenService = new Mock<ITokenServiceApiClient>();

            _token = new PrivilegedAccessToken {AccessCode = "code", ExpiryTime = DateTime.Today.AddDays(7)};

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _apprentice = _fixture.Create<ApprenticeEmploymentCheckModel>();
            _apprentice.MinDate = _fixture.Create<DateTime>();
            _apprentice.MaxDate = _apprentice.MinDate.AddMonths(-6);
        }

        [Fact]
        public async Task Then_The_TokenServiceApiClient_Is_Called()
        {
            // Arrange
            var employmentStatus = new EmploymentStatus {Employed = true};

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "PAYE",
                    _apprentice.NationalInsuranceNumber, _apprentice.MinDate, _apprentice.MaxDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);
            var request = _fixture.Create<ApprenticeEmploymentCheckMessageModel>();

            // Act
            await sut.IsNationalInsuranceNumberRelatedToPayeScheme(request);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Fact]
        public async Task Then_The_ApprenticeshipLevyApiClient_Is_Called_And_The_Response_Is_Returned()
        {
            // Arrange
            var employmentStatus = new EmploymentStatus {Employed = true};

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "PAYE",
                    _apprentice.NationalInsuranceNumber, _apprentice.MinDate, _apprentice.MaxDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);
            var request = _fixture.Build<ApprenticeEmploymentCheckMessageModel>()
                .With(x => x.NationalInsuranceNumber, _apprentice.NationalInsuranceNumber)
                .With(x => x.PayeScheme, "PAYE")
                .With(x => x.StartDateTime, _apprentice.MinDate)
                .With(x => x.EndDateTime, _apprentice.MaxDate)
                .Create();

            // Act
            var result = await sut.IsNationalInsuranceNumberRelatedToPayeScheme(request);

            // Assert
            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(_token.AccessCode, "PAYE",
                _apprentice.NationalInsuranceNumber, _apprentice.MinDate, _apprentice.MaxDate), Times.Exactly(1));
            Assert.Equal(employmentStatus.Employed, result.IsEmployed);
        }

        [Fact]
        public async Task And_The_ApprenticeshipLevyApiClient_Throws_An_ApiHttpException_Then_It_Returns_False()
        {
            // Arrange
            var exception = new ApiHttpException(404, "message", "uri", "body");

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "PAYE",
                    _apprentice.NationalInsuranceNumber, _apprentice.MinDate, _apprentice.MaxDate))
                .ThrowsAsync(exception);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);
            var request = _fixture.Build<ApprenticeEmploymentCheckMessageModel>()
                .With(x => x.NationalInsuranceNumber, _apprentice.NationalInsuranceNumber)
                .With(x => x.PayeScheme, "PAYE")
                .With(x => x.StartDateTime, _apprentice.MinDate)
                .With(x => x.EndDateTime, _apprentice.MaxDate)
                .Create();

            // Act
            var result = await sut.IsNationalInsuranceNumberRelatedToPayeScheme(request);

            // Assert
            Assert.False(result.IsEmployed);
        }
    }
}