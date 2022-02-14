using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.Hmrc;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.Hmrc;
using System.Threading.Tasks;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheckClientTests
{
    public class WhenCheckingEmploymentStatus
    {
        private readonly HmrcClient _sut;
        private readonly Fixture _fixture;
        private readonly Mock<IHmrcService> _hmrcServiceMock = new Mock<IHmrcService>();

        public WhenCheckingEmploymentStatus()
        {
            _fixture = new Fixture();

            var serviceProvider = new ServiceCollection()
                .AddLogging()
                .BuildServiceProvider();
            var factory = serviceProvider.GetService<ILoggerFactory>();
            var logger = factory.CreateLogger<HmrcClient>();

            _sut = new HmrcClient(_hmrcServiceMock.Object, logger);
        }

        [Test]
        public async Task Then_The_HmrcService_Is_Called()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture
                .Build<EmploymentCheckCacheRequest>()
                .With(r => r.Employed, true)
                .Create();

            _hmrcServiceMock
                .Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest))
                .ReturnsAsync(employmentCheckCacheRequest);

            // Act
            await _sut
                .CheckEmploymentStatus(employmentCheckCacheRequest);

            // Assert
            _hmrcServiceMock
                .Verify(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest), Times.AtLeastOnce);
        }

        [Test]
        public async Task And_The_HmrcService_Returns_An_EmploymentCheckCacheRequest_Then_It_Is_Returned()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture
                .Build<EmploymentCheckCacheRequest>()
                .With(r => r.Employed, true)
                .Create();

            _hmrcServiceMock
                .Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest))
                .ReturnsAsync(employmentCheckCacheRequest);

            // Act
            var result = await _sut
                .CheckEmploymentStatus(employmentCheckCacheRequest);

            // Assert
            Assert
                .AreEqual(employmentCheckCacheRequest, result);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_Null_Then_Null_IsReturned()
        {
            // Arrange
            var employmentCheckCacheRequest = _fixture
                .Build<EmploymentCheckCacheRequest>()
                .With(r => r.Employed, true)
                .Create();

            _hmrcServiceMock
                .Setup(x => x.IsNationalInsuranceNumberRelatedToPayeScheme(employmentCheckCacheRequest))
                .ReturnsAsync(() => null);

            // Act
            var result = await _sut
                .CheckEmploymentStatus(employmentCheckCacheRequest);

            //Assert
            result
                .Should()
                .BeNull();
        }
    }
}