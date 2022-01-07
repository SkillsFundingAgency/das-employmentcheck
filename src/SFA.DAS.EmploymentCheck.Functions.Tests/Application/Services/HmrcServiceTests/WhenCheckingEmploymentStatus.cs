using AutoFixture;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private Mock<ILogger<HmrcService>> _logger;
        private Mock<ITokenServiceApiClient> _tokenService;
        private PrivilegedAccessToken _token;
        private Functions.Application.Models.EmploymentCheckCacheRequest _apprentice;

        [SetUp]
        public void SetUp()
        {
            var fixture = new Fixture();
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _logger = new Mock<ILogger<HmrcService>>();
            _tokenService = new Mock<ITokenServiceApiClient>();

            _token = new PrivilegedAccessToken {AccessCode = "code", ExpiryTime = DateTime.Today.AddDays(7)};

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _apprentice = fixture.Create<Functions.Application.Models.EmploymentCheckCacheRequest>();
            _apprentice.MinDate = fixture.Create<DateTime>();
            _apprentice.MaxDate = _apprentice.MinDate.AddMonths(-6);
        }

        [Test]
        public async Task Then_The_TokenServiceApiClient_Is_Called()
        {
            // Arrange
            var employmentStatus = new EmploymentStatus {Employed = true};

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "PAYE",
                    _apprentice.Nino, _apprentice.MinDate, _apprentice.MaxDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);

            // Act
            await sut.IsNationalInsuranceNumberRelatedToPayeScheme(_apprentice);

            // Assert
            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Test]
        public async Task Then_The_ApprenticeshipLevyApiClient_Is_Called_And_The_Response_Is_Returned()
        {
            // Arrange
            var employmentStatus = new EmploymentStatus {Employed = true};

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, _apprentice.PayeScheme,
                    _apprentice.Nino, _apprentice.MinDate, _apprentice.MaxDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);

            // Act
            var result = await sut.IsNationalInsuranceNumberRelatedToPayeScheme(_apprentice);

            // Assert
            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(_token.AccessCode, _apprentice.PayeScheme,
                _apprentice.Nino, _apprentice.MinDate, _apprentice.MaxDate), Times.Exactly(1));
            Assert.AreEqual(employmentStatus.Employed, result.Employed);
        }
    }
}