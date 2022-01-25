using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SFA.DAS.EmploymentCheck.Functions.Application.Clients.EmploymentCheck;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmploymentCheck.Functions.Application.Services.EmploymentCheck;
using SFA.DAS.EmploymentCheck.Functions.Application.Models;
using System;

namespace SFA.DAS.EmploymentCheck.Functions.UnitTests.Application.Clients.EmploymentCheck.EmploymentCheckTests
{
    public class WhenGettingEmploymentCheckCacheRequest
    {
        private readonly Mock<IEmploymentCheckService> _employmentCheckService;
        private readonly Mock<ILogger<IEmploymentCheckClient>> _logger;
        private readonly Fixture _fixture;

        public WhenGettingEmploymentCheckCacheRequest()
        {
            _fixture = new Fixture();
            _employmentCheckService = new Mock<IEmploymentCheckService>();
            _logger = new Mock<ILogger<IEmploymentCheckClient>>();
        }

        [Test]
        public async Task Then_The_EmploymentCheckService_Is_Called()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(new EmploymentCheckCacheRequest());

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            await sut.GetEmploymentCheckCacheRequest();

            //Assert
            _employmentCheckService.Verify(x => x.GetEmploymentCheckCacheRequest(), Times.AtLeastOnce);
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_Null_EmploymentCheckCacheRequest_Then_Null_Is_Returned()
        {
            //Arrange
            _employmentCheckService.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync((EmploymentCheckCacheRequest)null);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheckCacheRequest();

            //Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task And_The_EmploymentCheckService_Returns_EmploymentCheckCacheRequest_Then_It_Is_Returned()
        {
            //Arrange
            var employmentCheckCacheRequest = new EmploymentCheckCacheRequest()
            {
                Id = 1,
                CorrelationId = new Guid("8f868d95-6313-4223-8026-53b8760f9abb"),
                Nino = "NI12345678",
                PayeScheme = "Paye1",
                LastUpdatedOn = new DateTime(2022, 4, 12, 16, 59, 15),
                MaxDate = new DateTime(2020, 10, 30, 20, 30, 28),
                MinDate = new DateTime(2023, 10, 9, 1, 33, 4),
                RequestCompletionStatus = 220,
                CreatedOn = new DateTime(2020, 10, 2, 23, 45, 53)
            };

            _employmentCheckService.Setup(x => x.GetEmploymentCheckCacheRequest())
                .ReturnsAsync(employmentCheckCacheRequest);

            var sut = new EmploymentCheckClient(_employmentCheckService.Object);

            //Act
            var result = await sut.GetEmploymentCheckCacheRequest();

            //Assert
            Assert.AreEqual(employmentCheckCacheRequest, result);
        }
    }
}