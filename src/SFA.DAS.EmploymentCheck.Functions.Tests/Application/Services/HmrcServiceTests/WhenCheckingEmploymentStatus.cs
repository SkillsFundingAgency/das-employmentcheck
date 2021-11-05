using System;
using HMRC.ESFA.Levy.Api.Client;
using HMRC.ESFA.Levy.Api.Types;
using HMRC.ESFA.Levy.Api.Types.Exceptions;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Models.Domain;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Helpers;
using SFA.DAS.EmploymentCheck.Functions.Mediators.Commands.CheckApprentice;
using SFA.DAS.TokenService.Api.Client;
using SFA.DAS.TokenService.Api.Types;
using Xunit;

namespace SFA.DAS.EmploymentCheck.Functions.Tests.Application.Services.HmrcServiceTests
{
    public class WhenCheckingEmploymentStatus
    {
        private readonly Mock<IApprenticeshipLevyApiClient> _apprenticeshipLevyService;
        private readonly Mock<ILoggerAdapter<HmrcService>> _logger;
        private readonly Mock<ITokenServiceApiClient> _tokenService;
        private readonly PrivilegedAccessToken _token;
        private readonly Apprentice _apprentice;
        private readonly CheckApprenticeCommand _command;

        public WhenCheckingEmploymentStatus()
        {
            _apprenticeshipLevyService = new Mock<IApprenticeshipLevyApiClient>();
            _logger = new Mock<ILoggerAdapter<HmrcService>>();
            _tokenService = new Mock<ITokenServiceApiClient>();
            
            _token = new PrivilegedAccessToken();
            _token.AccessCode = "code";
            _token.ExpiryTime = DateTime.Today.AddDays(7);

            _tokenService.Setup(x => x.GetPrivilegedAccessTokenAsync()).ReturnsAsync(_token);

            _apprentice = new Apprentice(1, 1, "1000001", 1000001, 10000001, 1, DateTime.Today.AddDays(-1),
                DateTime.Today.AddDays(1));
            _command = new CheckApprenticeCommand(_apprentice);
        }

        [Fact]
        public async void Then_The_TokenServiceApiClient_Is_Called()
        {
            //Arrange

            var employmentStatus = new EmploymentStatus();
            employmentStatus.Employed = true;

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "paye",
                    _apprentice.NationalInsuranceNumber, _command.Apprentice.StartDate, _command.Apprentice.EndDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);

            //Act

            await sut.IsNationalInsuranceNumberRelatedToPayeScheme("paye", _command, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            //Assert

            _tokenService.Verify(x => x.GetPrivilegedAccessTokenAsync(), Times.Exactly(1));
        }

        [Fact]
        public async void Then_The_ApprenticeshipLevyApiClient_Is_Called_And_The_Response_Is_Returned()
        {
            //Arrange

            var employmentStatus = new EmploymentStatus();
            employmentStatus.Employed = true;

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "paye",
                    _apprentice.NationalInsuranceNumber, _command.Apprentice.StartDate, _command.Apprentice.EndDate))
                .ReturnsAsync(employmentStatus);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);

            //Act

            var result = await sut.IsNationalInsuranceNumberRelatedToPayeScheme("paye", _command, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            //Assert

            _apprenticeshipLevyService.Verify(x => x.GetEmploymentStatus(_token.AccessCode, "paye",
                _apprentice.NationalInsuranceNumber, _command.Apprentice.StartDate, _command.Apprentice.EndDate), Times.Exactly(1));
            Assert.Equal(employmentStatus.Employed, result);
        }

        [Fact]
        public async void
            And_The_ApprenticeshipLevyApiClient_Throws_An_ApiHttpException_Then_It_Is_Logged_And_Returns_False()
        {
            //Arrange

            var exception = new ApiHttpException(404, "message", "uri", "body");

            _apprenticeshipLevyService.Setup(x => x.GetEmploymentStatus(_token.AccessCode, "paye",
                    _apprentice.NationalInsuranceNumber, _command.Apprentice.StartDate, _command.Apprentice.EndDate))
                .ThrowsAsync(exception);

            var sut = new HmrcService(_tokenService.Object, _apprenticeshipLevyService.Object, _logger.Object);

            //Act

            var result = await sut.IsNationalInsuranceNumberRelatedToPayeScheme("paye", _command, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            //Assert

            _logger.Verify(x => x.LogInformation($"{ DateTime.UtcNow } UTC HmrcService.IsNationalInsuranceNumberRelatedToPayeScheme(payScheme paye, [nationalInsuranceNumber for apprentice id {_command.Apprentice.Id}], startDate {DateTime.Today.AddDays(-1)}, endDate {DateTime.Today.AddDays(1)}): Exception caught - {exception.Message}. {exception.StackTrace}"));
            Assert.False(result);
        }
    }
}